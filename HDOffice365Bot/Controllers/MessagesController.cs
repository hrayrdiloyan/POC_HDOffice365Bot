using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using System.Configuration;

namespace HDOffice365Bot
{


    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            string strKeyPhrases = string.Empty;
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // Wait for message with attachment
                if (activity.Attachments.Count > 0)
                {
                    HttpClient client = new HttpClient();
                    // TODO: should deal with multiple attachments
                    byte[] attachmentContent = await client.GetByteArrayAsync(activity.Attachments[0].ContentUrl);
                    string strAttachmentContent = Encoding.UTF8.GetString(attachmentContent);
                    // Call cognitive services to find key phrases
                    strKeyPhrases = await GetKeyPhrases(strAttachmentContent);
                    var stringKeyPhrasesForEmail = "";
                    // Parse response from cognitive services and build reply email
                    if (!string.IsNullOrEmpty(strKeyPhrases))
                    {
                        ReturnRootObJect retOBJ = JsonConvert.DeserializeObject<ReturnRootObJect>(strKeyPhrases);
                        foreach (var key in retOBJ.documents[0].keyPhrases)
                            stringKeyPhrasesForEmail += "\"" + key + "\"" + ",";
                    }
                    // Build the reply email/message
                    StringBuilder replyMessage = new StringBuilder();
                    replyMessage.AppendLine($"Hi {activity.From.Name},<br/><br/>");
                    replyMessage.AppendLine($"For the document {activity.Attachments[0].Name}, we have found the following key phrases:<br/><br/>");
                    replyMessage.AppendLine(stringKeyPhrasesForEmail + "<br/><br/>");

                    // Put some hero card with possible links to the SharePoint sites
                    HeroCard card = new HeroCard();
                    card.Title = "Choose which SharePoint site to save document to(NOTE: LINKS WILL NOT WORK IN DEMO VERSION)";
                    card.Subtitle = "Sites are suggested based on attachment's key phrases";

                    CardAction actionOneDrive = new CardAction("openUrl", "My OneDrive");
                    CardAction actionSPSite1 = new CardAction("openUrl", "Marketing team site");
                    CardAction actionSPSite2 = new CardAction("openUrl", "UX team site");

                    List<CardAction> lstButtons = new List<CardAction>();
                    lstButtons.Add(actionOneDrive);
                    lstButtons.Add(actionSPSite1);
                    lstButtons.Add(actionSPSite2);
                    card.Buttons = lstButtons;

                    // Reply to the message
                    Activity reply = activity.CreateReply(replyMessage.ToString());
                    reply.Attachments = new List<Attachment>();
                    reply.Attachments.Add(card.ToAttachment());
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                else
                {
                    // If attachment is missing, give a notice via email
                    Activity reply = activity.CreateReply("Bot couldn't find any attachment to process.Please make sure your email has txt attachment.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
        async Task<string> GetKeyPhrases(string input)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            // Set headers to httpclient
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", ConfigurationManager.AppSettings["MSCognitiveServicesKey"]);

            // Prepare JSON body
            Document doc = new Document();
            doc.language = "en";
            doc.id = "1001";
            doc.text = input;
            List<Document> lstDocs = new List<Document>();
            lstDocs.Add(doc);
            RootObject ro = new RootObject();
            ro.documents = lstDocs;

            var content = new StringContent(JsonConvert.SerializeObject(ro), Encoding.UTF8, "application/json");
            HttpResponseMessage respone = await client.PostAsync("https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyphrases", content);
            return await respone.Content.ReadAsStringAsync();

        }
    }
}