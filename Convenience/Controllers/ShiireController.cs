using Convenience.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Convenience.Models.ViewModels.Shiire;
using Microsoft.EntityFrameworkCore;
using Convenience.Models.Interfaces;
using Convenience.Servises;
using Convenience.Models.Date.Shiire;
using Convenience.Models.Date.Chumon;

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
        public async Task<IActionResult> ShiireKey(string inChumonId) {
            List<ShiireJisseki> shiireJissekis = null;
            List<SokoZaiko> sokoZaikos = null;
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            ShiireJisseki shiireJisseki = shiire.ShiireToiawase(inChumonId);

            if (shiireJisseki == null) {
                shiireJissekis = shiire.ShiireCreate(inChumonId);
                sokoZaikos = shiire.ZaikoCreate(inChumonId);
            }
            else {
                shiireJisseki.SeqByShiireDate++;
            }
            //await _context.SaveChangesAsync();

            ShiireViewModel viewModel = new ShiireViewModel() {
                ShiireJisseki = shiireJissekis,
                SokoZaiko = sokoZaikos,
            };

            return View("ShiireView", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireView(List<ShiireJisseki> inshiireJisseki) {
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            List<ShiireJisseki> updatedShiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> updatedSokoZaikos = new List<SokoZaiko>();

            foreach (var shiireJisseki in inshiireJisseki) {
                var result = shiire.ShiireJissekiUpdate(shiireJisseki);
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
