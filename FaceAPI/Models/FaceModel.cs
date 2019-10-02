using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceAPI.Models
{
    public class FaceModel
    {
        public int Id { get; set; }

        public string name { get; set; }

        public string image { get; set; }
    }

    public class PersonalModel
    {
        public IList<Person> persons { get; set; }
        public int allowkey { get; set; }
        public List<string> req_folder { get; set; }

        public int folderkey { get; set; }
    }
}
