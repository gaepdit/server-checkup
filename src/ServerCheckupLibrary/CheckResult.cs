namespace ServerCheckupLibrary;

public interface ICheckResult
{
    Context ResultContext { get; }
    List<ResultMessage> Messages { get; }
}

public record CheckResult : ICheckResult
{
    public Context ResultContext { get; private set; } = Context.Success;
    public List<ResultMessage> Messages { get; } = [];

    public void AddMessage(Context messageContext, string text, string? details = null)
    {
        Messages.Add(new ResultMessage(messageContext, text, details));
        ResultContext = (Context)Math.Max((int)ResultContext, (int)messageContext);
    }

    public void AddMessage(ResultMessage message)
    {
        Messages.Add(message);
        ResultContext = (Context)Math.Max((int)ResultContext, (int)message.MessageContext);
    }
}

public record InfoResult : ICheckResult
{
    public Context ResultContext => Context.Info;
    public List<ResultMessage> Messages { get; } = new();

    public void AddMessage(string text, string? details = null) =>
        Messages.Add(new ResultMessage(Context.Info, text, details));
}

public enum Context
{
    Success,
    Info,
    Warning,
    Error,
}

public record ResultMessage(Context MessageContext, string Text, string? Details = null);
