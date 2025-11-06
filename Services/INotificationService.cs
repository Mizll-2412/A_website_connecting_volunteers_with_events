using khoaluantotnghiep.DTOs;

namespace khoaluantotnghiep.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDto>> GetNotificationsAsync(int userId, bool? read = null);
        Task<NotificationCountDto> GetNotificationCountAsync(int userId);
        Task<NotificationResponseDto> CreateNotificationAsync(CreateNotificationDto createDto);
        Task<NotificationResponseDto> UpdateNotificationStatusAsync(int userId, UpdateNotificationStatusDto updateDto);
        Task<bool> DeleteNotificationAsync(int userId, int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        
        // Helper methods for other services
        Task<bool> SendEventNotificationAsync(int eventId, string action, List<int> recipientIds);
        Task<bool> SendRegistrationNotificationAsync(int eventId, int userId, string action);
        Task<bool> SendEvaluationNotificationAsync(int evaluationId, int recipientId);
        Task<NotificationResponseDto> InviteVolunteerToEventAsync(int organizationUserId, InviteEventDto inviteDto);
    }
}
