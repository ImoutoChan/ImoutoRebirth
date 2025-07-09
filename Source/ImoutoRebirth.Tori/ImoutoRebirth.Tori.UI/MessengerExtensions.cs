using CommunityToolkit.Mvvm.Messaging;

namespace ImoutoRebirth.Tori.UI;

public static class MessengerExtensions
{
    public static void Register<TMessage, TRecipient>(
        this IMessenger messenger,
        TRecipient recipient,
        MessageHandler<TRecipient, TMessage> handler)
        where TMessage : class
        where TRecipient : class
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(handler);

        messenger.Register<TMessage>(
            recipient,
            (r, m) => handler((TRecipient)r, m));
    }
}
