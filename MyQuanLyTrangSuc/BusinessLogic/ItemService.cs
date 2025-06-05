using MyQuanLyTrangSuc.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class ItemService
    {
        private ItemRepository itemRepository;


        //singleton

        private static ItemService _instance;
        public static ItemService Instance => _instance ??= new ItemService();

        public ItemService()
        {
            itemRepository = new ItemRepository();
        }


    }
}
