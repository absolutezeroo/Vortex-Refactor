// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as
public interface IIlluminaChatBubbleWidget
{
    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::flipped
    bool Flipped { get; set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::userName
    string? UserName { get; set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::userId
    int UserId { get; set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::figure
    string? Figure { get; set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::numMessages
    int NumMessages { get; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::timeStamp
    double TimeStamp { get; set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::friendOnlineStatus
    bool FriendOnlineStatus { set; }

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::getMessage
    string? GetMessage(int index);

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::setMessage
    void SetMessage(int index, string message);

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::appendMessage
    void AppendMessage(string message, bool newLine = false, int confirmationId = 0);

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::setAwaitingConfirmationId
    void SetAwaitingConfirmationId(int messageIndex, int confirmationId);

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::clearAwaitingConfirmationId
    void ClearAwaitingConfirmationId(int messageIndex);

    /// @see habbo/window/widgets/IIlluminaChatBubbleWidget.as::getAwaitingConfirmationId
    int GetAwaitingConfirmationId(int messageIndex);
}
