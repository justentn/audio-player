using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace audio_player {
    public class MediaHandler {
        private static string _sqlConnection = "server=127.0.0.1;port=3306;userid=dev;password=devpassword;database=myDatabase";
        public MediaHandler() {
        }

        public void UploadMediaFile(IFormFile file) {
            byte[] result;
            System.IO.Directory.CreateDirectory(@".\Songs");
            var filePath = @".\Songs\"+file.FileName;

                using(var memoryStream = new MemoryStream())
                {
                    file.OpenReadStream().CopyTo(memoryStream);
                    result = memoryStream.ToArray();
                }
           
                using (FileStream fs = File.Create(filePath)){
                    fs.Write(result,0,result.Length);
                }

            MySqlConnection connection = new MySqlConnection(_sqlConnection);
            connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = "INSERT INTO songs(name,size,data) VALUES(@name,@size,@data) ON DUPLICATE KEY UPDATE data=@data, size=@size";
            cmd.Parameters.AddWithValue("@name", file.FileName);
            cmd.Parameters.AddWithValue("@data", result);
            cmd.Parameters.AddWithValue("@size", file.Length);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public Media DownloadMediaFile(string medianame) {
            MySqlConnection connection = new MySqlConnection(_sqlConnection);
            connection.Open();
            var cmd = new MySqlCommand();
            var media = new Media();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT name,data FROM songs WHERE name=@name;";
            cmd.Parameters.AddWithValue("@name", medianame);
            var reader = cmd.ExecuteReader();
            if(reader.Read()){
                media.Name = reader.GetString("name");
                media.Blob = reader.GetString("data");
            }
            reader.Close();
            connection.Close();
            return media;
        }

        public FileContentResult GetSong(string filename, int seek) {
            if(!File.Exists(@".\Songs\"+filename)){
                return null;
            }
            var myfile = System.IO.File.ReadAllBytes(@".\Songs\"+filename);
            seek = Math.Max(seek, 0);
            seek = Math.Min(seek, 100); 
            int fileLength = myfile.Length;
            int skipBytes = (fileLength*seek)/100;
            myfile = myfile.Skip(skipBytes).ToArray();
            return new FileContentResult(myfile, "audio/mpeg"); 
        }

        public string[] GetColumnFromName(string name, string column) {
            MySqlConnection connection = new MySqlConnection(_sqlConnection);
            connection.Open();
            var cmd = new MySqlCommand();
            string[] result = new string[1];
            cmd.Connection = connection;
            cmd.CommandText = "SELECT " + column + " FROM songs WHERE name=@name;";
            cmd.Parameters.AddWithValue("@name", name);
            var reader = cmd.ExecuteReader();
            if(reader.Read()){
                 result[0] = reader.GetString(0);
            }
            reader.Close();
            connection.Close();
            return result;
        }

        public string[] GetSongNames() {
            List<string> songnames = new List<string>();
            MySqlConnection connection = new MySqlConnection(_sqlConnection);
            connection.Open();
            var cmd = new MySqlCommand();
            var media = new Media();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT name FROM songs;";
            var reader = cmd.ExecuteReader();
            while(reader.Read()){
                songnames.Add(reader.GetString("name"));
            }
            reader.Close();
            connection.Close();
            return songnames.ToArray();
        }

        //https://localhost:5001/mediahandler/downloadmediachunk?name=easy.mp3?idx=1?size=100
        public string[] DownloadMediaChunk(string name, int idx, int size) {
            MySqlConnection connection = new MySqlConnection(_sqlConnection);
            string[] chunks = new string[1];
            connection.Open();
            var cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = "SELECT SUBSTR(data, @idx, @size) AS chunk FROM songs WHERE name=@name;";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@idx", idx);
            cmd.Parameters.AddWithValue("@size", size);
            var reader = cmd.ExecuteReader();
            if(reader.Read()){
                chunks[0] = reader.GetString(0);
            }
            reader.Close();
            connection.Close();
            return chunks;
        }
    }
}