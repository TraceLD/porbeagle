using OneOf;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Porbeagle;

public class MessageStyleViewManager : IContextAwareViewManager
{
    private readonly MessageContext _context;
    private readonly IDiscordRestChannelAPI _channelAPI;

    public MessageStyleViewManager(MessageContext context, IDiscordRestChannelAPI channelApi)
    {
        _context = context;
        _channelAPI = channelApi;
    }

    public async Task<Result<IMessage>> SendView(
        Snowflake channelID,
        IMessageView view,
        Optional<string> nonce = default,
        Optional<bool> isTTS = default,
        Optional<IAllowedMentions> allowedMentions = default,
        Optional<IMessageReference> messageReference = default,
        Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>> attachments = default,
        Optional<MessageFlags> flags = default,
        CancellationToken ct = default
    ) => await _channelAPI.CreateMessageAsync(
        channelID,
        view.Text,
        nonce,
        isTTS,
        view.Embeds,
        allowedMentions,
        messageReference,
        view.Components,
        view.Stickers,
        attachments,
        flags,
        ct
    );

    public async Task<Result<IMessage>> SendView
    (
        IMessageView view,
        Optional<string> nonce = default,
        Optional<bool> isTTS = default,
        Optional<IAllowedMentions> allowedMentions = default,
        Optional<IMessageReference> messageReference = default,
        Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>> attachments = default,
        Optional<MessageFlags> flags = default,
        CancellationToken ct = default
    ) => await _channelAPI.CreateMessageAsync(
        _context.ChannelID,
        view.Text,
        nonce,
        isTTS,
        view.Embeds,
        allowedMentions,
        messageReference,
        view.Components,
        view.Stickers,
        attachments,
        flags,
        ct
    );

    public async Task<Result<IMessage>> RespondWithView
    (
        IMessageView view,
        Optional<string> nonce = default,
        Optional<bool> isTTS = default,
        Optional<IAllowedMentions> allowedMentions = default,
        Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>> attachments = default,
        Optional<MessageFlags> flags = default,
        CancellationToken ct = default
    ) => await _channelAPI.CreateMessageAsync(
        _context.ChannelID,
        view.Text,
        nonce,
        isTTS,
        view.Embeds,
        allowedMentions,
        new MessageReference(_context.MessageID),
        view.Components,
        view.Stickers,
        attachments,
        flags,
        ct
    );
}