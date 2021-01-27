using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TesonetTask
{
    public class DataManager
    {
        private string _tokensLocation;
        private string _serversLocation;
        private string _localDbConnectionString;
        public string LastError { get; set; }
        HttpClient _httpClient;

        public DataManager(string tokensLocation, string serversLocation, string localDbConnectionString)
        {
            _tokensLocation = tokensLocation;
            _serversLocation = serversLocation;
            _localDbConnectionString = localDbConnectionString;
            _httpClient = new HttpClient();
        }

        private void Error(string message)
        {
            LastError = message;
            Logger.Log(message);
        }

        public List<Server> GetLocalData()
        {
            List<Server> existingServers = new List<Server>();
            using (SqlConnection conn = new SqlConnection(_localDbConnectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Servers", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        string name = reader["Name"].ToString();
                        if (!int.TryParse(reader["Distance"].ToString(), out int dist))
                        {
                            Error("Malformed data in database");
                            return null;
                        }
                        existingServers.Add(new Server() { name = name, distance = dist });
                    }
                    reader.Close();
                    return existingServers;
                }catch(Exception)
                {
                    Error("Error reading database");
                    return null;
                }
            }
        }

        public int PersistData(List<Server> data)
        {
            List<Server> existingServers = GetLocalData();
            if (existingServers == null)
                return -1;

            using (SqlConnection conn = new SqlConnection(_localDbConnectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Servers", conn);
                conn.Open();
                try
                {
                    foreach (Server s in existingServers)
                    {
                        var toRemove = data.FirstOrDefault(z => z.name.Equals(s.name));
                        if (toRemove != null)
                            data.Remove(toRemove);
                    }

                    DataTable tbl = new DataTable();
                    tbl.Columns.Add(new DataColumn("Id", typeof(Int32)));
                    tbl.Columns.Add(new DataColumn("Name", typeof(string)));
                    tbl.Columns.Add(new DataColumn("Distance", typeof(Int32)));

                    foreach(Server s in data)
                    {
                        DataRow dr = tbl.NewRow();
                        dr["Name"] = s.name;
                        dr["Distance"] = s.distance;
                        tbl.Rows.Add(dr);
                    }

                    SqlBulkCopy objbulk = new SqlBulkCopy(conn);
                    objbulk.DestinationTableName = "Servers";
                    objbulk.ColumnMappings.Add("Id", "Id");
                    objbulk.ColumnMappings.Add("Name", "Name");
                    objbulk.ColumnMappings.Add("Distance", "Distance");
                    objbulk.WriteToServer(tbl);
                    conn.Close();
                }
                catch(Exception)
                {
                    Error("Error writing to database");
                    return data.Count;
                }
            }

            return data.Count;
        }

        public async Task<string> GetAuthToken(string username, string password)
        {
            var values = new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            };

            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response;

            Logger.Log($"Sending token request to {_tokensLocation} username={username}");

            try
            {
                response = await _httpClient.PostAsync(_tokensLocation, content);
            }
            catch (Exception)
            {
                Error("Network error");
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            Dictionary<string, object> parsedResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

            if (parsedResponse.TryGetValue("token", out object token))
            {
                return token.ToString();
            }
            else if (parsedResponse.TryGetValue("message", out object message))
            {
                Error(message.ToString());
                return null;
            }
            else
            {
                Error("No valid response received");
                return null;
            }
        }

        public async Task<List<Server>> GetServerList(string authToken)
        {
            Logger.Log("Attempting to get server list...");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            string responseString;

            try
            {
                responseString = await _httpClient.GetStringAsync(_serversLocation);
            }
            catch (Exception)
            {
                Error("Network error");
                return null;
            }

            List<Server> serverList;

            try
            {
                serverList = JsonConvert.DeserializeObject<List<Server>>(responseString);
                return serverList;
            }
            catch (Exception)
            {
                Error("No valid response received");
                return null;
            }
        }
    }
}
