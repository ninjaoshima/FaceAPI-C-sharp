using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceAPI.Models;
using FaceAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FaceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {
        private readonly FaceServices _services;
        AzuraOwn azura;

        public FaceController(FaceServices services)
        {
            _services = services;
            FaceModel fm = new FaceModel();
            fm.Id = 1;
            fm.name = "Robinhood";
            _services.AddFaceItems(fm);
            //azura = new AzuraOwn();
        }
        [HttpGet]
        [Route("addfaceitem")]
        public ActionResult<FaceModel> AddPerson(FaceModel items)
        {
            //return Ok();
            var nameItems = _services.AddFaceItems(items);

            if(nameItems == null)
            {
                return NotFound();
            }

            return nameItems;
        }

        [HttpGet]
        [Route("getfaceitems")]
        public ActionResult<Dictionary<string, FaceModel>> GetFaceItems()
        {
            var faceItems = _services.GetFaceItems();
            if(faceItems.Count == 0)
            {
                return NotFound();
            }

            return faceItems;
        }

        [HttpGet]
        [Route("getnamelist")]

        public async Task<ActionResult<IList<string>>> GetNameList()
        {
            return await azura.GetNameList();
        }
    }
}