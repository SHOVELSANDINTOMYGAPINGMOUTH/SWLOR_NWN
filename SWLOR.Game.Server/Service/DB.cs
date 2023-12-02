using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NRediSearch;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;
using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Service.DBService;

namespace SWLOR.Game.Server.Service
{
    public static class DB
    {
        private static ApplicationSettings _appSettings;
        private static ConnectionMultiplexer _multiplexer;
        private static readonly Dictionary<string, EntityBase> _cachedEntities = new();

        private static RedisConnectionProvider _provider;
        private static readonly Dictionary<Type, string> _keyPrefixByType = new();
        private static readonly Dictionary<Type, Client> _searchClientsByType = new();
        private static readonly Dictionary<Type, List<string>> _indexedPropertiesByName = new();

        [NWNEventHandler("mod_preload")]
        public static void Load()
        {
            _appSettings = ApplicationSettings.Get();
            
            var options = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                EndPoints = { _appSettings.RedisIPAddress }
            };

            _multiplexer = ConnectionMultiplexer.Connect(options);
            _provider = new RedisConnectionProvider(_multiplexer);

            LoadEntities();

            // Runs at the end of every main loop. Clears out all data retrieved during this cycle.
            Internal.OnScriptContextEnd += () =>
            {
                _cachedEntities.Clear();
            };

            // CLI tools also use this class and don't have access to the NWN context.
            // Perform an environment variable check to ensure we're in the game server context before executing the event.
            var context = Environment.GetEnvironmentVariable("GAME_SERVER_CONTEXT");
            if (!string.IsNullOrWhiteSpace(context) && context.ToLower() == "true")
                ExecuteScript("db_loaded", OBJECT_SELF);
        }

        /// <summary>
        /// Processes the Redis Search index with the latest changes.
        /// </summary>
        /// <param name="entity"></param>
        private static void ProcessIndex(EntityBase entity)
        {
            var type = entity.GetType();

            var success = _provider.Connection.CreateIndex(type);

            if (success)
            {
                Console.WriteLine($"Successfully created index: {type}");
            }
            else
            {
                Console.WriteLine($"Failed to create index: {type}");
            }
        }

        /// <summary>
        /// When initialized, the assembly will be searched for all implementations of the EntityBase object.
        /// The KeyPrefix value of each of these will be stored into a dictionary for quick retrievals later.
        /// This is intended to abstract key building away from the consumer of this class.
        /// </summary>
        private static void LoadEntities()
        {
            var entityInstances = typeof(EntityBase)
                .Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(EntityBase)) && !t.IsAbstract && !t.IsGenericType)
                .Select(t => (EntityBase)Activator.CreateInstance(t));

            foreach (var entity in entityInstances)
            {
                var type = entity.GetType();
                ProcessIndex(entity);
                Console.WriteLine($"Registered type '{entity.GetType()}' using key prefix {type.Name}");
            }
        }

        /// <summary>
        /// Stores a specific object in the database by its Id.
        /// </summary>
        /// <typeparam name="T">The type of data to store</typeparam>
        /// <param name="entity">The data to store.</param>
        public static void Set<T>(T entity)
            where T : EntityBase
        {
            _provider.RedisCollection<T>().Update(entity);
            _cachedEntities[entity.Id] = entity;
        }

        /// <summary>
        /// Retrieves a specific object in the database by an arbitrary key.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve</typeparam>
        /// <param name="id">The arbitrary key the data is stored under</param>
        /// <returns>The object stored in the database under the specified key</returns>
        public static T Get<T>(string id)
            where T: EntityBase
        {
            if (_cachedEntities.ContainsKey(id))
            {
                return (T)_cachedEntities[id];
            }
            else
            {
                var entity = _provider.RedisCollection<T>().FindById(id);
                _cachedEntities[id] = entity;
                return entity;
            }
        }

        /// <summary>
        /// Retrieves the raw JSON of the object in the database by a given prefix and key.
        /// This can be useful for data migrations and one-time actions at server load.
        /// Do not use this for normal game play as it is slow.
        /// </summary>
        /// <param name="id">The arbitrary key the data is stored under</param>
        /// <returns>The raw json stored in the database under the specified key</returns>
        public static string GetRawJson<T>(string id)
        {
            var entity = _provider.RedisCollection<T>().FindById(id);

            if (entity == null)
                return string.Empty;

            return JsonConvert.SerializeObject(entity);
        }

        /// <summary>
        /// Returns true if an entry with the specified key exists.
        /// Returns false if not.
        /// </summary>
        /// <param name="id">The key of the entity.</param>
        /// <returns>true if found, false otherwise.</returns>
        public static bool Exists<T>(string id)
            where T : EntityBase
        {
            return _cachedEntities.ContainsKey(id) || 
                   _provider.RedisCollection<T>().Any();
        }

        /// <summary>
        /// Deletes an entry by a specified key.
        /// </summary>
        /// <typeparam name="T">The type of entity to delete.</typeparam>
        /// <param name="id">The key of the entity</param>
        public static void Delete<T>(string id)
            where T: EntityBase
        {
            var entity = _provider.RedisCollection<T>().FindById(id);
            _provider.RedisCollection<T>().Delete(entity);
            _cachedEntities.Remove(id);
        }

        /// <summary>
        /// Escapes tokens used in Redis queries.
        /// </summary>
        /// <param name="str">The string to escape</param>
        /// <returns>A string containing escaped tokens.</returns>
        public static string EscapeTokens(string str)
        {
            return str
                .Replace("@", "\\@")
                .Replace("!", "\\!")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("|", "\\|")
                .Replace("-", "\\-")
                .Replace("=", "\\=")
                .Replace(">", "\\>")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"");
        }

        public static IRedisCollection<Account> Accounts => _provider.RedisCollection<Account>();
        public static IRedisCollection<AreaNote> AreaNotes => _provider.RedisCollection<AreaNote>();
        public static IRedisCollection<AuthorizedDM> AuthorizedDMs => _provider.RedisCollection<AuthorizedDM>();
        public static IRedisCollection<Beast> Beasts => _provider.RedisCollection<Beast>();
        public static IRedisCollection<DMCreature> DMCreatures => _provider.RedisCollection<DMCreature>();
        public static IRedisCollection<Election> Elections => _provider.RedisCollection<Election>();
        public static IRedisCollection<InventoryItem> InventoryItems => _provider.RedisCollection<InventoryItem>();
        public static IRedisCollection<MarketItem> MarketItems => _provider.RedisCollection<MarketItem>();
        public static IRedisCollection<ModuleCache> ModuleCaches => _provider.RedisCollection<ModuleCache>();
        public static IRedisCollection<Player> Players => _provider.RedisCollection<Player>();
        public static IRedisCollection<PlayerBan> PlayerBans => _provider.RedisCollection<PlayerBan>();
        public static IRedisCollection<PlayerNote> PlayerNotes => _provider.RedisCollection<PlayerNote>();
        public static IRedisCollection<PlayerOutfit> PlayerOutfits => _provider.RedisCollection<PlayerOutfit>();
        public static IRedisCollection<PlayerShip> PlayerShips => _provider.RedisCollection<PlayerShip>();
        public static IRedisCollection<ServerConfiguration> ServerConfigurations => _provider.RedisCollection<ServerConfiguration>();
        public static IRedisCollection<WorldProperty> WorldProperties => _provider.RedisCollection<WorldProperty>();
        public static IRedisCollection<WorldPropertyCategory> WorldPropertyCategories => _provider.RedisCollection<WorldPropertyCategory>();
        public static IRedisCollection<WorldPropertyPermission> WorldPropertyPermissions => _provider.RedisCollection<WorldPropertyPermission>();


        /// <summary>
        /// Searches the Redis DB for records matching the query criteria.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="query">The query to run.</param>
        /// <returns>An enumerable of entities matching the criteria.</returns>
        public static IEnumerable<T> Search<T>(DBQuery<T> query)
            where T : EntityBase
        {
            var result = _searchClientsByType[typeof(T)].Search(query.BuildQuery());

            foreach (var doc in result.Documents)
            {
                // Remove the 'Index:' prefix.
                var recordId = doc.Id.Remove(0, 6);
                yield return _provider.RedisCollection<T>().FindById(recordId);
            }
        }

        /// <summary>
        /// Searches the Redis DB for raw JSON records matching the query criteria.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="query">The query to run.</param>
        /// <returns>An enumerable of raw json values matching the criteria.</returns>
        public static IEnumerable<string> SearchRawJson<T>(DBQuery<T> query)
            where T : EntityBase
        {
            var result = _searchClientsByType[typeof(T)].Search(query.BuildQuery());

            foreach (var doc in result.Documents)
            {
                // Remove the 'Index:' prefix.
                var recordId = doc.Id.Remove(0, 6);
                var entity = _provider.RedisCollection<T>().FindById(recordId);
                yield return JsonConvert.SerializeObject(entity);
            }
        }

        /// <summary>
        /// Searches the Redis DB for the number of records matching the query criteria.
        /// This only retrieves the number of records. Use Search() if you need the actual results.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="query">The query to run.</param>
        /// <returns>The number of records matching the query criteria.</returns>
        public static long SearchCount<T>(DBQuery<T> query)
            where T : EntityBase
        {
            var result = _searchClientsByType[typeof(T)].Search(query.BuildQuery(true));

            return result.TotalResults;
        }
    }
}
