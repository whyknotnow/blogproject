﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using NBlog.Web.Application.Infrastructure;
using NBlog.Web.Application.Service.Entity;
using Newtonsoft.Json;

namespace NBlog.Web.Application.Storage.Json
{
    public class JsonRepository : IRepository
    {
        private readonly RepositoryKeys _keys;
        private readonly HttpTenantSelector _tenantSelector;


        public string DataPath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("~/App_Data/" + _tenantSelector.Name);
            }
        }


        public JsonRepository(RepositoryKeys keys, HttpTenantSelector tenantSelector)
        {
            _keys = keys;
            _tenantSelector = tenantSelector;
        }


        public TEntity Single<TEntity>(object key) where TEntity : class, new()
        {
            var filename = key.ToString();
            var recordPath = Path.Combine(DataPath, typeof(TEntity).Name, filename + ".json");
            var json = File.ReadAllText(recordPath);
            var item = JsonConvert.DeserializeObject<TEntity>(json);
            return item;
        }


        public IEnumerable<TEntity> All<TEntity>() where TEntity : class, new()
        {
            var folderPath = Path.Combine(DataPath, typeof(TEntity).Name);
            var filePaths = Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly);

            var list = new List<TEntity>();
            foreach (var path in filePaths)
            {
                var jsonString = File.ReadAllText(path);
                var entity = JsonConvert.DeserializeObject<TEntity>(jsonString);
                list.Add(entity);
            }

            return list;
        }


        public void Save<TEntity>(TEntity item) where TEntity : class, new()
        {
            var json = JsonConvert.SerializeObject(item, Formatting.Indented);
            var folderPath = GetEntityPath<TEntity>();
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            var filename = _keys.GetKeyValue(item);
            var recordPath = Path.Combine(folderPath, filename + ".json");

            File.WriteAllText(recordPath, json);
        }


        public bool Exists<TEntity>(object key) where TEntity : class, new()
        {
            var folderPath = GetEntityPath<TEntity>();
            var recordPath = Path.Combine(folderPath, key + ".json");
            return File.Exists(recordPath);
        }

        // todo: not in IRepository? should be?
        public void DeleteAll<TEntity>()
        {
            var folderPath = GetEntityPath<TEntity>();
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }


        public void Delete<TEntity>(object key) where TEntity : class, new()
        {
            var folderPath = GetEntityPath<TEntity>();
            var recordPath = Path.Combine(folderPath, key + ".json");
            File.Delete(recordPath);
        }


        // todo: need a TEntity + TKey version of this too that does Path.Combine(folderPath, key + ".json");
        private string GetEntityPath<TEntity>()
        {
            return Path.Combine(DataPath, typeof(TEntity).Name);
        }
    }
}
