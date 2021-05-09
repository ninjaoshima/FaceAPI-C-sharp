using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FaceAPI.Controllers;
using FaceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceAPI.Models
{

    public class AzuraOwn
    {
        private string subscriptionKey;
        // Add your Face endpoint to your environment variables.
        private string faceEndpoint;

        private readonly IFaceClient faceClient;
        // The list of detected faces.
        private IList<DetectedFace> faceList;
        // The list of descriptions for the detected faces.
        private string[] faceDescriptions;
        // The resize factor for the displayed image.
        private double resizeFactor;

        private const string defaultStatusBarText =
            "Place the mouse pointer over a face to see the face description.";
        private string personalGroupId = "quangtask";
        private string imageFolder = @"C:\image_folder";

        private readonly FaceServices _services;


        public AzuraOwn(FaceServices services)
        {
            subscriptionKey = "e721666785724a9a947d5d05772fd6ea";
            faceEndpoint = "https://frservice1.cognitiveservices.azure.com";//Environment.GetEnvironmentVariable("FACE_ENDPOINT");
            faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey),
            new System.Net.Http.DelegatingHandler[] { });

            if (Environment.GetEnvironmentVariable("FACE_IMAGEFOLDER") != null)
            {
                imageFolder = @Environment.GetEnvironmentVariable("FACE_IMAGEFOLDER");
            }

            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
            else
            {
                //     MessageBox.Show(faceEndpoint,
                //    "Invalid URI", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            _services = services;
        }

        public async Task<string> getImage()
        {
            string filepath = "C:\\1.jpg";
            Uri fileUri = new Uri(filepath);

            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.None;
            bitmapImage.UriSource = fileUri;
            bitmapImage.EndInit();

            //FacePhoto.Source = bitmapImage;
            faceList = await UploadAndDetectFaces(filepath);

            return faceList.Count.ToString();
        }

        public async Task<string> FaceRegister(Guid personalId, string photo)
        {
            //string filePath = "C:\\2.jpg";
            string base64 = photo.Substring(photo.IndexOf(',') + 1);
            byte[] data = Convert.FromBase64String(base64);
            try
            {
                using (Stream s = new MemoryStream(data))
                {
                    //faceClient.PersonGroup.
                    PersistedFace persisted = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(
                        personalGroupId, personalId, s);
                    //persisted.
                    return persisted.PersistedFaceId.ToString();
                }
            }catch(Exception)
            {
                return "Failed";
            }
        }

        public async Task<string> FaceDeleteAndCreate()
        {
            try
            {
                await faceClient.PersonGroup.DeleteAsync(personalGroupId);
                await CreatePersonalGroup();
                return "OK";
            }catch(Exception e)
            { return e.Message; }
        }

        public async Task<string> CreatePersonalGroup()
        {
            try
            {
                await faceClient.PersonGroup.CreateAsync(personalGroupId, "Staff");
                return "Ok";
            }catch(Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> getPersonId(string username)
        {
            try
            {
                var persons = await faceClient.PersonGroupPerson.ListAsync(personalGroupId);
                foreach (var person in persons)
                {
                    if (person.Name == username)
                    {
                        return "Failed";
                    }
                }

                Person friend1 = await faceClient.PersonGroupPerson.CreateAsync(
                        personalGroupId, username);

                return friend1.PersonId.ToString();
            }catch(Exception)
            {
                return "Failed";
            }
        }

        public async Task<string> TrainPersonGroup()
        {
            try
            {
                await faceClient.PersonGroup.TrainAsync(personalGroupId);
                TrainingStatus trainingStatus = null;
                while (true)
                {
                    trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personalGroupId);

                    if (trainingStatus.Status != TrainingStatusType.Running)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
                return "Ok";
            }catch(Exception e)
            {
                return e.Message;
            }
        }

        public async Task<List<string>> GetNameList()
        {
            List<string> nameList = new List<string>();
            try
            {
                var persons = await faceClient.PersonGroupPerson.ListAsync(personalGroupId);

                PersonalModel pm = new PersonalModel();
                pm.persons = persons;
                _services.SetPersons(pm);
                foreach(var person in persons)
                {
                    nameList.Add(person.Name);
                }
                return nameList;
            }
            catch(Exception)
            {
                return nameList;
            }
        }

        //public async Task<string> get

        public async Task<ImageIndexModel> getIdentifyFace(int index)
        {
            DirectoryInfo d = new DirectoryInfo(imageFolder + @"\" + _services.GetCurrentFolder());
            string logfile = imageFolder + @"\log.txt";
            FileInfo[] files = d.GetFiles("*.jpg");
            string testImageFile = files[index].FullName;
            string time_str = files[index].Name.Split('.')[0];
            if (index == files.Length - 1 && _services.GetFolderKey() >= _services.GetReqFolder().Count -1)
            {
                _services.SetAllowKey(0);
            }
            try
            {
                using (Stream s = File.OpenRead(testImageFile))
                {
                    var faces = await faceClient.Face.DetectWithStreamAsync(s);
                    IList<Guid> faceIds = faces.Select<DetectedFace, Guid>(face => (Guid)face.FaceId).ToArray();//.Select(face => face.FaceId).ToList();
                    int result_index = faceIds.Count / 10 + 1;
                    List<IList<Guid>> faceIdContainer = new List<IList<Guid>>(result_index);
                    int c_index = 0, r_index = 0;
                    foreach(Guid guid in faceIds)
                    {
                        c_index++;
                        if(c_index / 10 >= r_index)
                        {
                            r_index++;
                            faceIdContainer.Add(new List<Guid>());
                        }
                        faceIdContainer[c_index / 10].Add(guid);    
                    }
                    List<IList<IdentifyResult>> results1 = new List<IList<IdentifyResult>>();
                    foreach(IList<Guid> guids in faceIdContainer)
                    {
                        results1.Add(await faceClient.Face.IdentifyAsync(guids, personalGroupId));
                    }
                    
                    List<string> personName = new List<string>();
                    PersonalModel persons = _services.GetPersonal();
                    List<string> history = new List<string>();
                    foreach(IList<IdentifyResult> identifies in results1)
                    {
                        foreach (var identifyResult in identifies)
                        {
                            Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                            if (identifyResult.Candidates.Count == 0)
                            {
                                personName.Add("No one identified");
                            }
                            else
                            {
                                // Get top 1 among all candidates returned
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                foreach (var person in persons.persons)
                                {
                                    if (person.PersonId == identifyResult.Candidates[0].PersonId)
                                    {
                                        personName.Add(person.Name);
                                        CultureInfo provider = CultureInfo.InvariantCulture;
                                        //string dt1 = DateTime.Now.ToString("yyyyMMdd_HHmmss_FFF");
                                        DateTime dt = DateTime.ParseExact(time_str, "yyyyMMdd_HHmmss_FFF", provider); //.Parse(time_str.Split('_')[0]);
                                        string his = dt.ToString("yyyy/MM/dd HH:mm:ss ") + person.Name + " appeared";
                                        history.Add(his);
                                        if (!File.Exists(logfile))
                                        {
                                            File.WriteAllText(logfile, his);
                                        }
                                        else
                                        {
                                            his = $"{Environment.NewLine}{his}";
                                            File.AppendAllText(logfile, his);
                                        }
                                        break;
                                    }
                                }
                                //var person = new { Name = ""};// await faceClient.PersonGroupPerson.GetAsync(personalGroupId, candidateId);
                                //Console.WriteLine("Identified as {0}", person.Name);
                                //return "Idnetified as " + person.Name;

                            }
                        }
                    }
                    

                    Image image = Image.FromFile(testImageFile);
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        Pen brush = new Pen(Brushes.Red, 2);
                        Font drawFont = new Font("Arial", 16);
                        for (int i = 0; i < faces.Count; i++)
                        {
                            g.DrawRectangle(brush, new Rectangle(faces[i].FaceRectangle.Left, faces[i].FaceRectangle.Top, faces[i].FaceRectangle.Width, faces[i].FaceRectangle.Height));
                            //attr.Add(faces[i].Attributes);
                            g.DrawString(personName[i], drawFont, Brushes.Blue, faces[i].FaceRectangle.Left, faces[i].FaceRectangle.Top - 30);
                        }

                        string base64String = null;

                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat);
                            byte[] bytes = m.ToArray();
                            base64String = Convert.ToBase64String(bytes);
                            base64String = "data:image/jpg;base64," + base64String;
                        }

                        ImageIndexModel indexM = new ImageIndexModel();
                        indexM.index = index;
                        indexM.src = base64String;
                        indexM.history = history;
                        
                        return indexM;

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ImageIndexModel indexM = new ImageIndexModel();
                indexM.index = index;
                indexM.src = "Failed";
                //indexM.history = history;
                return indexM;
            }

            
        }

        public int GetImageCount(int index)
        {
            _services.SetFolderKey(index);
            return Directory.GetFiles(imageFolder + @"\" + _services.GetReqFolder()[index], "*.jpg", SearchOption.TopDirectoryOnly).Length; 
        }

        public AllowKeyModel GetAllowKey()
        {
            //return _services.GetAllowKey();
            int key = _services.GetAllowKey();
            if(key != 1)
            {
                return new AllowKeyModel(key, new List<string>());
            }
            return new AllowKeyModel(key, _services.GetReqFolder());
        }

        public async Task<ResponseModel> GetFaceAll(RequestBody request)
        {
            await GetNameList();

            _services.SetReqFolder(request.parameters);
            List<string> param = new List<string>();
            ResultFaceModel rfm;
            Dictionary<string, Dictionary<string, ResultFaceModel>> dic = new Dictionary<string, Dictionary<string, ResultFaceModel>>();
            List<string> errors = new List<string>();

            
            //ResultFolderModel rfolderModel = new ResultFolderModel();
            foreach (string subfolder in request.parameters)
            {
                DirectoryInfo d = new DirectoryInfo(imageFolder + @"\" + subfolder);
                FileInfo[] files = d.GetFiles("*.jpg");
                List<ResultFolderModel> listFolder = new List<ResultFolderModel>();
                Dictionary<string, ResultFaceModel> drmodel = new Dictionary<string, ResultFaceModel>();
                param.Add(subfolder);
                foreach (FileInfo file in files)
                {
                    
                    try
                    {
                        using (Stream s = File.OpenRead(file.FullName))
                        {
                            var face_s = await faceClient.Face.DetectWithStreamAsync(s);

                            rfm = new ResultFaceModel();
                            rfm.image = file.FullName;
                            rfm.faces = face_s;

                            //ResultFolderModel rfolder = new ResultFolderModel();
                            //rfolder.folder = file.Name.Split('.')[0];
                            //rfolder.model = rfm;
                            //listFolder.Add(rfolder);
                            drmodel[file.Name.Split('.')[0]] = rfm;
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message);
                    }
                    
                }
                dic[subfolder] = drmodel;
            }
            
            
            _services.SetAllowKey(1);
            ResponseModel rpm = new ResponseModel();
            rpm.Parameters = param;
            rpm.Result = dic;
            rpm.Method = "getface";
            rpm.Error = errors;
            return rpm;
        }

        //public async string GetImages(int index)
        //{
        //    DirectoryInfo d = new DirectoryInfo(imageFolder);
        //    FileInfo[] files = d.GetFiles("*.jpg");
        //    using(Stream s = File.OpenRead(files[index].FullName))
        //    {
        //        var faces = await faceClient.Face.DetectWithStreamAsync(s);
        //        IList<Guid> faceIds = faces.Select<DetectedFace, Guid>(face => (Guid)face.FaceId).ToArray();

        //        var result = await faceClient.Face.IdentifyAsync(faceIds, personalGroupId);
        //    }
        //    return "Failed";
        //}

        private async Task<IList<DetectedFace>> UploadAndDetectFaces(string imageFilePath)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);
                    return faceList;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                //MessageBox.Show(f.Message);
                return new List<DetectedFace>();
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Error");
                return new List<DetectedFace>();
            }
        }

    }


    public class AllowKeyModel
    {
        public int allowkey { get; set; }
        public List<string> request { get; set; }

        public AllowKeyModel(int key, List<string> req)
        {
            allowkey = key;
            request = req;
        }

    }

}
