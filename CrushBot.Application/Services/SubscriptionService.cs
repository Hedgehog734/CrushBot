using CrushBot.Application.Adapters;
using CrushBot.Application.Facades.Interfaces;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrushBot.Application.Services
{
    public class SubscriptionService(
        IUserDistributedFacade userFacade,
        IAgeService ageService,
        IAppSettings appSettings,
        ISubscription subSettings)
        : ISubscriptionService
    {
        public async Task ProcessMemberAsync(ChatMemberUpdated member)
        {
            if (member.Chat.Id == subSettings.ChannelId)
            {
                var userId = member.NewChatMember.User.Id;
                var status = member.NewChatMember.Status;
                await userFacade.UpdateUserAsync(userId, dto => AdjustBasedOnSubscription(dto, status));
            }
        }

        public async Task<bool> IsUserSubscribedAsync(long userId, ITelegramClient client,
            CancellationToken cancellationToken = default)
        {
            var member = await client.GetChatMemberAsync(subSettings.ChannelId, userId,
                cancellationToken);

            return member.Status is ChatMemberStatus.Creator or ChatMemberStatus.Administrator
                or ChatMemberStatus.Member;
        }

        public BotUser CutDataBySubscription(BotUser user)
        {
            var userDto = user.ToDto();
            CutUserData(userDto);

            return userDto.ToEntity();
        }

        private void AdjustBasedOnSubscription(BotUserDto user, ChatMemberStatus status)
        {
            if (status is ChatMemberStatus.Creator or ChatMemberStatus.Administrator
                or ChatMemberStatus.Member)
            {
                user.IsSubscribed = true;
                user.ShowEmoji = true;
            }

            if (status is ChatMemberStatus.Left or ChatMemberStatus.Kicked
                or ChatMemberStatus.Restricted)
            {
                user.IsSubscribed = false;
                CutUserData(user);
            }
        }

        private void CutUserData(BotUserDto user)
        {
            CutFilter(user);
            CutDescription(user);
            CutPhotos(user);
            ReplaceLongVideo(user);
        }

        private void CutFilter(BotUserDto user)
        {
            var (minAge, maxAge) = ageService.GetAllowedRange(user);

            if (user.Filter != null)
            {
                if (user.Filter.AgeAfter < minAge)
                {
                    user.Filter.AgeAfter = minAge;
                }

                if (user.Filter.AgeUntil > maxAge)
                {
                    user.Filter.AgeUntil = maxAge;
                }
            }
        }

        private static void CutDescription(BotUserDto user)
        {
            const int allowedLength = BaseDescriptionState.LenMaxNoSub;

            if (user.Description is { Length: > allowedLength })
            {
                user.Description = user.Description[..allowedLength];
            }
        }

        private static void CutPhotos(BotUserDto user)
        {
            const int allowedCount = BaseChoosePhotoState.MaxPhotoCountNoSub;

            if (user.PhotoIds.Count > allowedCount)
            {
                user.PhotoIds.RemoveRange(allowedCount, user.PhotoIds.Count - allowedCount);
            }
        }

        private void ReplaceLongVideo(BotUserDto user)
        {
            const int allowedDuration = BaseChooseVideoState.MaxDurationNoSub;

            if (user.VideoId is { Length: > allowedDuration })
            {
                user.VideoId = null;

                if (user.PhotoIds.Count == 0)
                {
                    var photo = appSettings.PlaceholderImageId;
                    user.PhotoIds.Add(photo);
                }
            }
        }
    }
}
