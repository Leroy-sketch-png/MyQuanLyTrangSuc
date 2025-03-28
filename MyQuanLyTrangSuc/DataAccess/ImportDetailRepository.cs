using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess {
    internal class ImportDetailRepository {
        MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        public List<ImportDetail> LoadImportDetailsFromDatabase(Import SelectedImportRecord) {
            List<ImportDetail> ImportDetailsFromDb = context.ImportDetails.Where(ii => ii.ImportId == SelectedImportRecord.ImportId).ToList();
            //foreach (ImportDetail ii in ImportDetailsFromDb) {
                //ImportDetails.Add(ii);
            //}
            return ImportDetailsFromDb;
        }

    }
}
