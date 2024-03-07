using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Convenience.Data;
using Convenience.Models.Interfaces;
using Convenience.Models.Date.Chumon;
using Convenience.Models.ViewModels.Chumon;
using Convenience.Servises;

namespace Convenience.Controllers
{
    public class ChumonController : Controller {
        private readonly ConvenienceContext _context;

        public ChumonController(ConvenienceContext context) {
            _context = context;
        }

        public async Task<IActionResult> ChumonKey() {
            ChumonKeyViewModel chumonKeyViewModel = new ChumonKeyViewModel {
                ShiireSakiList = await _context.ShiireSakiMasters
                .OrderBy(s => s.ShiireSakiId)
                .Select(s => new SelectListItem { Value = s.ShiireSakiId, Text = s.ShiireSakiId + " " + s.ShiireSakiKaisya })
                .ToListAsync(), // データベースからリストを取得し、ToListAsync() を使って非同期で取得します
                ChumonDate = DateOnly.FromDateTime(DateTime.Today)
            };

            return View(chumonKeyViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChumonKey(string inShiireSakiId, DateOnly inChumonDate, ChumonViewModel viewModel) {
            IChumon chumonService = new ChumonService(_context);
            ModelState.Clear();

            //string inShireSakiId, DateOnly inChumonDate
            string ShiireSakiId = inShiireSakiId;
            DateOnly ChumonDate = inChumonDate;

            ChumonJisseki chumonJisseki = chumonService.ChumonToiawase(ShiireSakiId, ChumonDate);

            if (chumonJisseki == null) {
                chumonJisseki = chumonService.ChumonCreate(ShiireSakiId);
            }
            else {
                chumonJisseki = chumonService.ChumonUpdate(chumonJisseki);
            }

            viewModel.ChumonJisseki = chumonJisseki;


            return View("ChumonView", viewModel);
        }

        public async Task<IActionResult> ChumonView(ChumonViewModel inchumonViewModel) {
            IChumon chumonService = new ChumonService(_context);
            ModelState.Clear();

            // inchumonViewModelからChumonJissekiを取得
            ChumonJisseki chumonJisseki = inchumonViewModel.ChumonJisseki;

            ChumonJisseki updatedChumonJisseki = chumonService.ChumonUpdate(chumonJisseki);

            // ビューモデルの注文実績に更新後の注文実績をセット
            inchumonViewModel.ChumonJisseki = updatedChumonJisseki;

            // 仕入先マスタと商品マスタの関連データをロード
            foreach (var meisai in inchumonViewModel.ChumonJisseki.ChumonJissekiMeisais) {
                await _context.Entry(meisai).Reference(m => m.ShiireMaster).LoadAsync();
                await _context.Entry(meisai.ShiireMaster).Reference(s => s.ShohinMaster).LoadAsync();
            }

            // ここでデータベースに変更を保存
            await _context.SaveChangesAsync();

            // chumonViewModelを定義し、必要な操作を行う
            ChumonViewModel chumonViewModel = new ChumonViewModel() {
                ChumonJisseki = updatedChumonJisseki,
                Remark = "更新しました"
            };

            return View(chumonViewModel);
        }



    }
}
