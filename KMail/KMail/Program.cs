using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace KMail
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Validating Email Paramters...");

            EmailParameterCollection parameters = EmailParameterCollection.Parse(args);

            Console.WriteLine("Email Parameters Valid? {0}", (parameters.IsValid ? "Yes" : "No"));

            bool sendResult = false;

            if (parameters.IsValid) {
                Console.WriteLine("Sending Email to Provided Recipients...");
                sendResult = SendEmail(parameters);
            }
            else {
                Console.WriteLine("Unable to Parse Recipient List...");
            }

            Console.WriteLine("Exit Code: {0}", (sendResult ? 0 : 1));
            
        }

        private static bool SendEmail(EmailParameterCollection parameters)
        {

            try
            {
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("systememails@krodzone.com", "YR30mEf%");
                SmtpClient server = new SmtpClient("smtp.office365.com", 587);

                server.UseDefaultCredentials = false;
                server.DeliveryMethod = SmtpDeliveryMethod.Network;
                server.Credentials = credentials;
                server.EnableSsl = true;

                MailMessage msg = new MailMessage();

                foreach (MailAddress address in ((EmailParameter<List<MailAddress>>)parameters[EmailParameterType.RecipientList]).Value)
                {
                    msg.To.Add(address);
                }

                msg.From = new MailAddress("systememails@krodzone.com", "Krodzone Automated (Do Not Reply)");
                msg.Subject = ((EmailParameter<string>)parameters[EmailParameterType.Subject]).Value;
                msg.Body = ((EmailParameter<string>)parameters[EmailParameterType.Body]).Value;
                msg.IsBodyHtml = ((EmailParameter<bool>)parameters[EmailParameterType.Format]).Value;

                server.Send(msg);

                Console.WriteLine("Message Successfully Sent...");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Encountered:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return false;

            }

            return true;

        }

    }

    public enum EmailParameterType
    {
        RecipientList = 0,
        Subject = 2,
        Format = 3,
        Body = 4
    }

    public class EmailParameterCollection
    {

        #region Local Variables
        protected readonly Dictionary<EmailParameterType, EmailParameter> _Items;
        protected bool _IsValid;
        #endregion

        #region Constructor
        public EmailParameterCollection()
        {
            _Items = new Dictionary<EmailParameterType, EmailParameter>();
            _IsValid = false;
        }
        #endregion

        #region Properties
        public EmailParameter this[EmailParameterType key]
        {
            get
            {
                if (_Items.ContainsKey(key))
                {
                    return _Items[key];
                }

                return null;
            }
        }

        public bool IsValid
        {
            get { return _IsValid; }
        }
        #endregion

        #region Public Shared Methods
        public static EmailParameterCollection Parse(string[] args)
        {
            EmailParameterCollection parameters = null;

            if (args != null && args.Length > 0)
            {
                string emailPattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

                parameters = new EmailParameterCollection();

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Substring(0, 1) == "-" && args[i].IndexOf(" ") == -1)
                    {
                        switch (args[i].ToUpper())
                        {
                            case "-TO":

                                if (args.Length > (i + 1) && Regex.IsMatch(args[i + 1], emailPattern, RegexOptions.IgnoreCase))
                                {
                                    List<MailAddress> recipients = new List<MailAddress>();

                                    foreach (Match match in Regex.Matches(args[i + 1], emailPattern, RegexOptions.IgnoreCase))
                                    {
                                        recipients.Add(new MailAddress(match.Value));
                                    }

                                    parameters.Add(new EmailParameter<List<MailAddress>>() { ParamterType = EmailParameterType.RecipientList, Value = recipients });

                                }

                                i++;

                                break;
                            case "-SUBJECT":
                                
                                if (args.Length > (i + 1))
                                {
                                    parameters.Add(new EmailParameter<string>() { ParamterType = EmailParameterType.Subject, Value = args[i + 1] });
                                }

                                i++;

                                break;
                            case "-HTML":
                                
                                parameters.Add(new EmailParameter<bool>() { ParamterType = EmailParameterType.Format, Value = true });

                                break;
                            case "-TEXT":

                                parameters.Add(new EmailParameter<bool>() { ParamterType = EmailParameterType.Format, Value = false });

                                break;
                            case "-BODY":
                                
                                if (args.Length > (i + 1))
                                {
                                    StringBuilder body = new StringBuilder();

                                    while ((i + 1) < args.Length)
                                    {
                                        i++;

                                        if (args[i].Substring(0, 1) != "-")
                                        {
                                            body.Append(string.Format("{0} ", args[i]));
                                        }
                                        else
                                        {
                                            i--;
                                            break;
                                        }
                                        
                                    }

                                    parameters.Add(new EmailParameter<string>() { ParamterType = EmailParameterType.Body, Value = body.ToString().Trim() });
                                }

                                break;
                        }
                    }
                }

                parameters.Validate();

            }

            return parameters;

        }
        #endregion

        #region Public Methods
        public void Add(EmailParameter parameter)
        {

            if (parameter != null)
            {
                if (_Items.ContainsKey(parameter.ParamterType))
                {
                    _Items[parameter.ParamterType] = parameter;
                }
                else
                {
                    _Items.Add(parameter.ParamterType, parameter);
                }
            }

        }

        public void Validate()
        {

            if (_Items.Count > 0 && _Items.ContainsKey(EmailParameterType.RecipientList))
            {

                if (!_Items.ContainsKey(EmailParameterType.Format))
                {
                    Add(new EmailParameter<bool>() { ParamterType = EmailParameterType.Format, Value = true });
                }

                if (!_Items.ContainsKey(EmailParameterType.Subject))
                {
                    Add(new EmailParameter<string>() { ParamterType = EmailParameterType.Subject, Value = "(No Subject Provided)" });
                }

                if (!_Items.ContainsKey(EmailParameterType.Body))
                {
                    Add(new EmailParameter<string>() { ParamterType = EmailParameterType.Body, Value = "(No Body Provided)" });
                }

                _IsValid = true;

            }
            else
            {
                _IsValid = false;
            }

        }
        #endregion

    }

    public abstract class EmailParameter
    {

        #region Constructor
        public EmailParameter() { }
        #endregion

        #region Properties
        public EmailParameterType ParamterType { get; set; }
        #endregion

    }

    public class EmailParameter<T> : EmailParameter
    {

        #region Constructor
        public EmailParameter()
            : base()
        {

        }
        #endregion

        #region Properties
        public T Value { get; set; }
        #endregion

    }

}
