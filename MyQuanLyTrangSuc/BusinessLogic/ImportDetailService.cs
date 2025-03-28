using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic {
    internal class ImportDetailService {
        private readonly ImportDetailRepository importDetailRepository;
        private static ImportDetailService instance;
        public static ImportDetailService Instance { get { 
                if (instance == null) {
                    instance = new ImportDetailService(); 
                }
                return instance;
            }
        }
        private ImportDetailService() { 
            importDetailRepository = new ImportDetailRepository();
        }

        public void LoadImportDetailsFromDatabase(Import SelectedImportRecord, ObservableCollection<ImportDetail> ImportDetails) {
            List<ImportDetail> ImportDetailsFromDb = importDetailRepository.LoadImportDetailsFromDatabase(SelectedImportRecord);
            foreach (ImportDetail ii in ImportDetailsFromDb) {
                ImportDetails.Add(ii);
            }
        }

    }
}
