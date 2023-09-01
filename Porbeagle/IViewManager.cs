using OneOf;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Porbeagle;

public interface IViewManager
{
    /// <summary>
    /// Posts a view to a given channel.
    /// </summary>
    /// <param name="channelID">The ID of the channel.</param>
    /// <param name="view">The view to be posted.</param>
    /// <param name="nonce">A nonce that can be used for optimistic message sending.</param>
    /// <param name="isTTS">Whether the message is a TTS message.</param>
    /// <param name="allowedMentions">An object describing the allowed mention types.</param>
    /// <param name="messageReference">A reference to another message.</param>
    /// <param name="attachments">
    /// The attachments to associate with the response. Each file may be a new file in the form of
    /// <see cref="FileData"/>, or an existing one that should be retained in the form of a
    /// <see cref="IPartialAttachment"/>. If this request edits the original message, then any attachments not
    /// mentioned in this parameter will be deleted.
    /// </param>
    /// <param name="flags">The message flags.</param>
    /// <param name="ct">The cancellation token for this operation.</param>
    /// <returns>A creation result which may or may not have succeeded.</returns>
    Task<Result<IMessage>> SendView<TView, TViewModel>(
        Snowflake channelID,
        TViewModel vm,
        Optional<string> nonce = default,
        Optional<bool> isTTS = default,
        Optional<IAllowedMentions> allowedMentions = default,
        Optional<IMessageReference> messageReference = default,
        Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>> attachments = default,
        Optional<MessageFlags> flags = default,
        CancellationToken ct = default
    ) where TView : IMessageView<TView, TViewModel>;
}