using Newtonsoft.Json;
using System;
using System.IO;
using AstmDriver.Behavior.Host;

namespace AstmDriver.Commons
{
    public class ConfigRepository
    {
        string _path = string.Empty;
        DriverConfig _config = null;
        public DriverConfig Config
        {
            get
            {
                return _config;
            }
        }

        public void SetConfig(DriverConfig config)
        {
            _config = config;
        }
        public ConfigRepository()
        {
            _path = Path.Combine(Environment.CurrentDirectory, "driver.config.json");
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                _config = JsonConvert.DeserializeObject<DriverConfig>(json, 
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                
                if (json.Contains("Version") == false)
                {
                    switch (_config.LowLevel)
                    {
                        default: case "ASTM":
                            _config.HostType = nameof(AstmHost);
                            break;
                        case "BeckmanCoulter":
                            _config.HostType = nameof(BeckmanCoulterHost);
                            break;
                        case "Metascan":
                            _config.HostType = nameof(MetascanHost);
                            break;
                        case "Miura":
                            _config.HostType = nameof(MiuraHost);
                            break;
                        case "Aspect":
                            _config.HostType = nameof(MllpHost);
                            break;
                    }

                    _config.ServerVersion = 3;
                    _config.Version = Globals.Version;
                    SaveConfig();
                }
                else
                {
                    if (_config.Version != Globals.Version)
                    {
                        //Сюда пишем поведение изменение конфига, если вдруг мы добавили новый параметр
                        _config.Version = Globals.Version;
                        SaveConfig();
                    }
                }
            }
            else
            {
                _config = new DriverConfig();
                SaveConfig();
            }
        }

        public void SaveConfig()
        {
            var json = JsonConvert.SerializeObject(_config);
            File.WriteAllText(_path, json);
        }
    }
}
