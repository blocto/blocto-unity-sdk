using System.Text;

namespace Blocto.Sdk.Ethereum.Model.Rpc
{
    public class BodyTemplate
    {
        static BodyTemplate()
        {
            BodyTemplate._template = @"{""id"":{ID},""method"":""{MethodName}"",""params"":[{Parameters}]}";
        }
        
        private static string _template;

        public static string Id { get; set; }
        public static string MethodName { get; set; }

        public static string Parameters { get; set; }

        public static string BodyContent
        {
            get {
                    var tmp = new StringBuilder(BodyTemplate._template);
                    tmp.Replace("{ID}", BodyTemplate.Id);
                    tmp.Replace("{MethodName}", BodyTemplate.MethodName);
                    tmp.Replace("{Parameters}", BodyTemplate.Parameters);
                    return tmp.ToString();
                }
        }
    }
}