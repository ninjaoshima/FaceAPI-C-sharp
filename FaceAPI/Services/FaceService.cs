using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceAPI.Models;

namespace FaceAPI.Services
{
    public class FaceService : FaceServices
    {
        private readonly Dictionary<string, FaceModel> _inventroyItems;
        private PersonalModel _persons;

        public FaceService()
        {
            _inventroyItems = new Dictionary<string, FaceModel>();
        }
        public FaceModel AddFaceItems(FaceModel items)
        {
            _inventroyItems.Add(items.name, items);

            return items;
            //throw new NotImplementedException();
        }

        public Dictionary<string, FaceModel> GetFaceItems()
        {

            return _inventroyItems;
        }

        public void SetPersons(PersonalModel person)
        {
            _persons = person;
            //throw new NotImplementedException();
        }

        public PersonalModel GetPersonal()
        {
            return _persons;
        }

        public void SetAllowKey(int key)
        {
            _persons.allowkey = key;
        }

        public int GetAllowKey()
        {
            return _persons.allowkey;
        }

        public void SetReqFolder(List<string> req)
        {
            //throw new NotImplementedException();
            _persons.req_folder = req;
        }

        public List<string> GetReqFolder()
        {
            //throw new NotImplementedException();
            return _persons.req_folder;
        }

        public void SetFolderKey(int key)
        {
            _persons.folderkey = key;
        }

        public int GetFolderKey()
        {
            return _persons.folderkey;
        }

        public string GetCurrentFolder()
        {
            return _persons.req_folder[_persons.folderkey];
        }
    }
}
