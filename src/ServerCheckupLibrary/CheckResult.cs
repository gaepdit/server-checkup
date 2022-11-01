namespace CheckServerSetup.Checks;

public class CheckResult
{
    public Context ResultContext { get; private set; } = Context.Success;
    public List<Message> Messages { get; } = new();

    public void AddMessage(Context context, string text, string? details = null)
    {
        Messages.Add(new Message(context, text, details));
        ResultContext = (Context)Math.Max((int)ResultContext, (int)context);
    }

    public class Message
    {
        public Message(Context messageContext, string text, string? details = null)
        {
            MessageContext = messageContext;
            Text = text;
            Details = details;
        }

        public Context MessageContext { get; }
        public string Text { get; }
        public string? Details { get; }
    }

    public static string ClassSuffix(Context context) => context switch
    {
        Context.Success => "success",
        Context.Info => "info",
        Context.Warning => "warning",
        Context.Error => "danger",
        _ => string.Empty,
    };

    public enum Context
    {
        Success,
        Info,
        Warning,
        Error,
    }
}
