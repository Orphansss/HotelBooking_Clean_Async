using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;

namespace HotelBooking.Reqnroll.Support
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> items;

        public InMemoryRepository(IEnumerable<T> seed)
        {
            items = seed?.ToList() ?? new List<T>();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<T>>(items);
        }

        public Task<T> GetAsync(int id)
        {
            var entity = items.FirstOrDefault(i => GetId(i) == id);
            return Task.FromResult(entity);
        }

        public Task AddAsync(T entity)
        {
            items.Add(entity);
            return Task.CompletedTask;
        }

        public Task EditAsync(T entity)
        {
            var index = items.FindIndex(i => GetId(i) == GetId(entity));
            if (index >= 0)
            {
                items[index] = entity;
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(int id)
        {
            items.RemoveAll(i => GetId(i) == id);
            return Task.CompletedTask;
        }

        private static int GetId(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
            {
                return -1;
            }

            var value = idProperty.GetValue(entity);
            return value is int id ? id : -1;
        }
    }
}