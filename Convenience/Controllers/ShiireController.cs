using Convenience.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Convenience.Models.ViewModels.Shiire;
using Microsoft.EntityFrameworkCore;
using Convenience.Models.Interfaces;
using Convenience.Servises;
using Convenience.Models.Date.Shiire;

namespace Convenience.Controllers {
    public class ShiireController : Controller {
        private readonly ConvenienceContext _context;

        public ShiireController(ConvenienceContext context) {
            _context = context;
        }

        public async Task<IActionResult> ShiireKey() {
            //データベースからリストを取得し、ToListAsync() を使って非同期で取得
            ShiireKeyViewModel shiireKeyViewModel = new ShiireKeyViewModel() {
                ChumonIdList = await _context.ChumonJissekis
                .OrderBy(c => c.ChumonId)
                .Select(c => new SelectListItem { Value = c.ChumonId, Text = c.ChumonId })
                .ToListAsync()
            };

            ShiireKeyViewModel viewModel = shiireKeyViewModel;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireKey(string inChumonId, ShiireViewModel viewModel) {
            List<ShiireJisseki> shiireJissekis;
            List<SokoZaiko> sokoZaikos = null;
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            string ChumonId = inChumonId;

            shiireJissekis = shiire.ShiireToiawase(ChumonId);

            if (shiireJissekis == null) {
                shiireJissekis = shiire.ShiireCreate(ChumonId);
                sokoZaikos = shiire.ZaikoCreate(ChumonId);
            }
            else {
                SokoZaiko sokoZaiko = null;
                sokoZaikos = new List<SokoZaiko>();
                foreach (var shiireJisseki in shiireJissekis) {
                    sokoZaiko = _context.SokoZaikos.Where(s => s.ShiirePrdId == shiireJisseki.ShiirePrdId).First();
                    sokoZaikos.Add(sokoZaiko);
                }
            }

            viewModel.ShiireJisseki = shiireJissekis;
            viewModel.SokoZaiko = sokoZaikos;
           

            return View("ShiireView", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireView(ShiireViewModel inshiireViewModel) {
            List<ShiireJisseki> shiireJissekis = inshiireViewModel.ShiireJisseki;
            List<SokoZaiko> sokoZaikos = inshiireViewModel.SokoZaiko;
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            List<ShiireJisseki> updatedShiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> updatedSokoZaikos = new List<SokoZaiko>();

            foreach (var (shiireJisseki, sokoZaiko) in shiireJissekis.Zip(sokoZaikos, (a, b) => (a, b))) {
                ShiireJisseki ShiireJisseki = shiire.ChumonZanBalance(shiireJisseki);
                SokoZaiko SokoZaiko = shiire.SokoZaikoSuCal(shiireJisseki);
                var result = shiire.ShiireJissekiUpdate(shiireJisseki, sokoZaiko);
                updatedShiireJissekis.Add(result.shiireJisseki);
                updatedSokoZaikos.Add(result.sokoZaiko);
            };

            // データベースに変更を保存する
            await _context.SaveChangesAsync();

            ShiireViewModel viewModel = new ShiireViewModel() {
                ShiireJisseki = updatedShiireJissekis,
                SokoZaiko = updatedSokoZaikos,
                Remark = "仕入登録完了"
            };
            return View(viewModel);
        }
    }
}
