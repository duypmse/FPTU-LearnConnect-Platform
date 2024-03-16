using AutoMapper;
using BAL.Models;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface INotificationRepository : IBaseRepository<NotificationDTO> {
        IEnumerable<object> GetNotificationsByUserId(int userId);
        public void Create(string title, string description, int[] userIds);
        public void MarkNotificationsAsRead(int userId);

    }
    public class NotificationRepository : INotificationRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public NotificationRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<NotificationDTO> GetList()
        {
            var _list = _context.Notifications.ToList();
            var _listDTO = _mapper.Map<List<NotificationDTO>>(_list);
            return _listDTO;
        }

        public NotificationDTO Get(int id)
        {
            var _object = _context.Notifications.Find(id);
            var _objectDTO = _mapper.Map<NotificationDTO>(_object);
            return _objectDTO;
        }

        public NotificationDTO Add(NotificationDTO _objectDTO)
        {
            var _object = _mapper.Map<Notification>(_objectDTO);
            _context.Notifications.Add(_object);
            _context.SaveChanges();
            return _mapper.Map<NotificationDTO>(_object);
        }

        public int Update(int id, NotificationDTO _objectDTO)
        {
            var existingObject = _context.Notifications.Find(id);
            if (existingObject == null)
            {
                return 0;
            }

            _mapper.Map(_objectDTO, existingObject);
            return 1;
        }


        public int Delete(int id)
        {
            Notification _object = _context.Notifications.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.Notifications.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.Notifications.Any(e => e.Id == id);
            return _isExist;
        }
        /*public IEnumerable<NotificationDTO> GetNotificationsByUserId(int userId)
        {
            var notifications = _context.Notifications.Where(n => n.UserId == userId).ToList();
            var notificationDTOs = _mapper.Map<List<NotificationDTO>>(notifications);
            //MarkNotificationsAsRead(userId);
            return notificationDTOs;
        }
        public void MarkNotificationsAsRead(int userId)
        {
            var notificationsToUpdate = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();

            foreach (var notification in notificationsToUpdate)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
        }*/

        public IEnumerable<object> GetNotificationsByUserId(int userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Id)
                .ToList();
            var notificationDTOs = _mapper.Map<List<NotificationDTO>>(notifications);

            var countUnRead = _context.Notifications.Count(n => n.UserId == userId && n.IsRead == false);

            var returnNoti = new List<object>();

            var data = new
            {
                CountUnRead = countUnRead,
                Notification = notificationDTOs
            };
            returnNoti.Add(data);
            return returnNoti;
        }
        public void MarkNotificationsAsRead(int userId)
        {
            var notificationsToUpdate = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToList();

            foreach (var notification in notificationsToUpdate)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
        }

        public void Create(string title, string description, int[] userIds)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var notification = new Notification
                    {
                        Title = title,
                        Description = description,
                        TimeStamp = DateTime.UtcNow.AddHours(7),
                        IsRead = false,
                        UserId = userId
                    };

                    _context.Notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error at Create(): " + ex);
            }
        }

    }
}
