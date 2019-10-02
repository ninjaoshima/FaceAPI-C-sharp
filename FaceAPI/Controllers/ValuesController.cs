using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceAPI.Models;
using Microsoft.AspNetCore.Cors;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using FaceAPI.Services;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace FaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        AzuraOwn azura;
        public ValuesController(FaceServices services)
        {
            //_services = services;
            azura = new AzuraOwn(services);
            //Redirect()
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("getnamelist")]
        public async Task<ActionResult<IList<string>>> GetNameList()
        {
            return await azura.GetNameList();
        }

        [HttpPost]
        [Route("checkname")]
        public async Task<ActionResult> GetOwnerById([FromBody] Dictionary<string, string> value)
        {
            string id = await azura.getPersonId(value["name"]);

            return new JsonResult(new { id = id});
        }

        [HttpPost]
        [Route("addface")]
        public async Task<ActionResult> AddFace([FromBody] AddFaceModel addFace)
        {
            string resFace = await azura.FaceRegister(addFace.personId, addFace.photo);
            return new JsonResult(new { persisted = resFace});
        }

        [HttpGet]
        [Route("newgroup")]
        public async Task<ActionResult> NewGroup()
        {
            return Ok(await azura.FaceDeleteAndCreate());
        }

        [HttpGet]
        [Route("traingroup")]
        public async Task<ActionResult> TrainGroup()
        {
            return new JsonResult(new { status = await azura.TrainPersonGroup()});
        }

        [HttpGet]
        [Route("getimagecount")]
        public ActionResult GetImageCount(int index)
        {
            return new JsonResult(new { count = azura.GetImageCount(index) });
        }

        [HttpPost]
        [Route("getimages")]
        public async Task<ActionResult> GetImages([FromBody] ImageIndexModel src)
        {
            //int index =
            return new JsonResult( await azura.getIdentifyFace(src.index));
        }

        [HttpGet]
        [Route("getface")]
        public async Task<ActionResult> GetFace([FromBody] RequestBody request)
        {
            return new JsonResult(await azura.GetFaceAll(request));
        }

        [HttpGet]
        [Route("getallowkey")]
        public ActionResult GetAllowKey()
        {
            //return new JsonResult(new { status = azura.GetAllowKey() });
            return new JsonResult(new { status = "0" });
        }
    }

    public class AddFaceModel
    {
        public Guid personId { get; set; }

        public string photo { get; set; }
    }

    public class ImageIndexModel
    {
        public int index { get; set; }

        public string src { get; set; }

        public IList<string> history { get; set; }
    }

    //public class ResultModel
    //{
    //    public Dictionary<string, ResultFaceModel> Result { get; set; }
    //}

    public class ResultFaceModel
    {
        public string image { get; set; }
        
        public IList<DetectedFace> faces { get; set; }
    }

    public class ResultFolderModel
    {
        public string folder;
        public ResultFaceModel model;
    }

    public class ResponseModel
    {
        public List<string> Parameters { get; set; }
        public string Method { get; set; }
        public Dictionary<string, Dictionary<string, ResultFaceModel>> Result { get; set; }
        public List<string> Error { get; set; }
    }

    public class RequestBody
    {
        public string method { get; set; }

        public List<string> parameters { get; set; }
    }
}
