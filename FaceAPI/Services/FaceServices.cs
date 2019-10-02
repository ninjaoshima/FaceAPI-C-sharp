using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceAPI.Models;

namespace FaceAPI.Services
{
    public interface FaceServices
    {
        FaceModel AddFaceItems(FaceModel items);
        Dictionary<string, FaceModel> GetFaceItems();

        void SetPersons(PersonalModel person);
        PersonalModel GetPersonal();

        void SetAllowKey(int key);
        int GetAllowKey();

        void SetReqFolder(List<string> req);

        List<string> GetReqFolder();
        void SetFolderKey(int key);
        int GetFolderKey();

        string GetCurrentFolder();
    }
}
