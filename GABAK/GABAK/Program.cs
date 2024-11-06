//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System;
using System.Windows.Forms;
// for use of app 
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
// for HTTP GET Requests
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static GABAK.Program;

namespace GABAK
{
    internal static class Program
    {
        // Define classes to match the Json structure
        public class UserData
        {
            public object Module_Task_ID { get; set; }
            public List<int?> Pick_List_ID { get; set; } = new List<int?>();
            public List<double> x { get; set; } = new List<double>();
            public List<double> y { get; set; } = new List<double>();
            public List<double> z { get; set; } = new List<double>();
        }

        public class userInformation
        {
            public string User_ID { get; set; }
            public string[] Sessions { get; set; }  // Renamed to match JSON
        }
        // Matching Json structure with the root class
        public class RootObject
        {
            public UserData data { get; set; }
            public bool Final_Chunk { get; set; }
        }

        // Define classes to match the XML structure
        public class WarehouseData
        {
            public List<RackLocation> RacksLocation { get; set; } = new List<RackLocation>();
            public Dictionary<string, List<ObjectiveLocation>> ObjectivesLocation { get; set; } = new Dictionary<string, List<ObjectiveLocation>>();
            public double WarehouseWidth { get; set; }
            public double WarehouseDepth { get; set; }
            public double CenterX { get; set; }
            public double CenterY { get; set; }
            public double RackWidth { get; set; }
            public double RackDepth { get; set; }
        }

        public class RackLocation
        {
            public double X1 { get; set; }
            public double X2 { get; set; }
            public double X3 { get; set; }
            public double X4 { get; set; }
            public double Y1 { get; set; }
            public double Y2 { get; set; }

            public double Y3 { get; set; }
            public double Y4 { get; set; }
            public double Angle { get; set; }
        }

        public class ObjectiveLocation
        {
            public double X { get; set; }
            public double Y { get; set; }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public static userInformation FetchUserInformation(string emailInput)
        {
            using (var client = new HttpClient())
            {
                var userEndpoint = new Uri($"https://www.gokhanozden.com/augmentedwarehouse/warp/php/get_all_user_sessions_from_email.php?email={emailInput}");
                var userResult = client.GetAsync(userEndpoint).Result;
                var userJson = userResult.Content.ReadAsStringAsync().Result;

                userInformation userInformationObject = JsonConvert.DeserializeObject<userInformation>(userJson);
                
                if (userInformationObject.User_ID != null)
                {
                    Console.WriteLine("UserID: " + userInformationObject.User_ID);
                    Console.WriteLine("Sessions: " + string.Join(", ", userInformationObject.Sessions));
                }
                else
                {
                    Console.WriteLine("UserID Data is null or JSON deserialization failed.");
                }

                return userInformationObject;
            }
        }

        public static (UserData, WarehouseData) FetchData(string sessionID)
        {
            using (var client = new HttpClient())
            {
                var offset = 0;
                var limit = 100;
                bool isFinalChunk = false;
                UserData combinedUserData = new UserData();

                while (!isFinalChunk) {
                    var endpoint = new Uri($"https://www.gokhanozden.com/augmentedwarehouse/warp/testing/php/get_user_coordinates.php?sessionID={sessionID}&offset={offset}&limit={limit}");
                    // Fetch data from HTTP GET request and disregard async and do it synchronously
                    var result = client.GetAsync(endpoint).Result;
                    var json = result.Content.ReadAsStringAsync().Result;
                    // Deserialize JSON into the RootObject class
                    RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(json);


                    if (rootObject?.data != null)
                    {
                        combinedUserData.Module_Task_ID = rootObject.data.Module_Task_ID;
                        combinedUserData.Pick_List_ID.AddRange(rootObject.data.Pick_List_ID);
                        combinedUserData.x.AddRange(rootObject.data.x);
                        combinedUserData.y.AddRange(rootObject.data.y);
                        combinedUserData.z.AddRange(rootObject.data.z);

                        //Console.WriteLine($"Offset: {offset}, Final_Chunk: {rootObject.Final_Chunk}");
                        isFinalChunk = rootObject.Final_Chunk;
                        offset = offset + 100;
                    }
                    else
                    {
                        Console.WriteLine("UserData is null or JSON deserialization failed.");
                        break;
                    }
                }

                // Handle fetching data from map (ARD file)
                var mapEndpoint = new Uri($"https://www.gokhanozden.com/augmentedwarehouse/warp/php/get_module_task_info.php?moduleTaskId={combinedUserData.Module_Task_ID}");
                // Fetch data from HTTP GET request and disregard async and do it synchronously
                var mapResult = client.GetAsync(mapEndpoint).Result;
                var mapString = mapResult.Content.ReadAsStringAsync().Result;
                string extractedXML = CleanXML(mapString);

                var warehouseData = ParseARD(extractedXML);
                // DEBUG OUTPUT

                Console.WriteLine("Warehouse Width: " + warehouseData.WarehouseWidth);
                Console.WriteLine("Warehouse Depth: " + warehouseData.WarehouseDepth);
                Console.WriteLine("Number of Racks: " + warehouseData.RacksLocation.Count);
                return (combinedUserData, warehouseData);
            }
        }

        public static string CleanXML(string jsonString)
            {
                var jsonObject = JObject.Parse(jsonString);
                string xmlString = jsonObject["ARD"].ToString();

                xmlString = xmlString.Replace("\\\"", "\"")
                                     .Replace("\\\\", "\\")
                                     .Replace("\\/", "/")
                                     .Replace("\\n", "\n")
                                     .Replace("\\r", "\r")
                                     .Trim();
                return xmlString;
            }

        public static WarehouseData ParseARD(string fullARD)
        {
            var warehouseData = new WarehouseData();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(fullARD);

                // should select the first node that matches the xml path "//warehouse"
                // if not null, parse the warehouse dimensions / storage location dimensions from XML attributes and convert to meters
                var warehouseNode = xmlDoc.SelectSingleNode("//warehouse");
                if (warehouseNode != null)
                {
                    warehouseData.WarehouseWidth = double.Parse(warehouseNode.Attributes["width"].Value) / 3.281;
                    warehouseData.WarehouseDepth = double.Parse(warehouseNode.Attributes["depth"].Value) / 3.281;
                    warehouseData.CenterX = double.Parse(warehouseNode.Attributes["center_x"].Value) / 3.281;
                    warehouseData.CenterY = double.Parse(warehouseNode.Attributes["center_y"].Value) / 3.281;
                    warehouseData.RackWidth = double.Parse(warehouseNode.Attributes["storagelocationwidth"].Value) / 3.281;
                    warehouseData.RackDepth = double.Parse(warehouseNode.Attributes["storagelocationdepth"].Value) / 3.281;
                }

                // should select all nodes that matches the xml path "//region"
                var regionNodes = xmlDoc.SelectNodes("//region");
                foreach (XmlNode regionNode in regionNodes)
                {
                    // parse the angle attribute from the region node & select all "storagelocation" nodes within current region node
                    var angle = double.Parse(regionNode.Attributes["angle"].Value);
                    var storageLocationNodes = regionNode.SelectNodes("storagelocation");
                    foreach (XmlNode storageLocationNode in storageLocationNodes)
                    {
                        // parse the X and Y coords of each storage location and convert to meters
                        var x1 = double.Parse(storageLocationNode.Attributes["x1"].Value) / 3.281;
                        var x2 = double.Parse(storageLocationNode.Attributes["x2"].Value) / 3.281;
                        var x3 = double.Parse(storageLocationNode.Attributes["x3"].Value) / 3.281;
                        var x4 = double.Parse(storageLocationNode.Attributes["x4"].Value) / 3.281;
                        var y1 = double.Parse(storageLocationNode.Attributes["y1"].Value) / 3.281;
                        var y2 = double.Parse(storageLocationNode.Attributes["y2"].Value) / 3.281;
                        var y3 = double.Parse(storageLocationNode.Attributes["y3"].Value) / 3.281;
                        var y4 = double.Parse(storageLocationNode.Attributes["y4"].Value) / 3.281;

                        // add the storage location data
                        warehouseData.RacksLocation.Add(new RackLocation
                        {
                            X1 = x1,
                            X2 = x2,
                            X3 = x3,
                            X4 = x4,
                            Y1 = y1,
                            Y2 = y2,
                            Y3 = y3,
                            Y4 = y4,
                            Angle = angle
                        });
                    }
                }

                // should select all nodes that matches the xml path "//picklist"
                var picklistNodes = xmlDoc.SelectNodes("//picklist");
                foreach (XmlNode picklistNode in picklistNodes)
                {
                    // get id of current picklist & init a list for storing pick locations for current picklist ID
                    var id = picklistNode.Attributes["id"].Value;
                    warehouseData.ObjectivesLocation[id] = new List<ObjectiveLocation>();

                    // select all "picklocation" nodes within the current picklist ID
                    var pickLocationNodes = picklistNode.SelectNodes("picklocation");
                    foreach (XmlNode pickLocationNode in pickLocationNodes)
                    {
                        // parse the X and Y coordinates of each pick location & convert to meters
                        var pickListX = double.Parse(pickLocationNode.Attributes["x"].Value) / 3.281;
                        var pickListY = double.Parse(pickLocationNode.Attributes["y"].Value) / 3.281;

                        // add the pick location data to the ObjectivesLocation dict for the current picklist ID
                        warehouseData.ObjectivesLocation[id].Add(new ObjectiveLocation
                        {
                            X = pickListX,
                            Y = pickListY
                        });
                    }
                }
                return warehouseData;
            }
        }
    }
