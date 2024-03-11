using Convenience.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Convenience.Models.ViewModels.Shiire;
using Microsoft.EntityFrameworkCore;
using Convenience.Models.Interfaces;
using Convenience.Servises;
using Convenience.Models.Date.Shiire;
using Convenience.Models.Date.Chumon;
using Convenience.Models.ViewModels.Chumon;

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
            string ChumonId = inChumonId;
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            List<ChumonJissekiMeisai> chumonJissekiMeisais = _context.ChumonJissekiMeisais
                .Where(c => c.ChumonId == ChumonId)
                .Include(c => c.ChumonJisseki)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShiireSakiMaster)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShohinMaster)
                .OrderBy(c => c.ShiirePrdId)
                .ToList();

            List<ShiireJisseki> shiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> sokoZaikos = new List<SokoZaiko>();

            DateOnly ShiireDate = DateOnly.FromDateTime(DateTime.Today);

            foreach (var chumonJissekiMeisai in chumonJissekiMeisais) {
                ShiireJisseki shiireJisseki = shiire.ShiireToiawase(ChumonId, ShiireDate);
                if (shiireJisseki == null) {  //実績がなかった場合（当日）
                    shiireJisseki = shiire.ShiireCreate(chumonJissekiMeisai);
                }
                else {  //仕入実績があった場合（当日）
                    //時間のみ変更
                    shiireJisseki.ShiireDateTime = DateTime.UtcNow;
                }
                shiireJissekis.Add(shiireJisseki);
                //仕入マスタ検索
                ShiireMaster shiireMaster = _context.ShiireMasters
                    .Where(s => s.ShiireSakiId == chumonJissekiMeisai.ShiireSakiId && s.ShiirePrdId == chumonJissekiMeisai.ShiirePrdId && s.ShohinId == chumonJissekiMeisai.ShohinId)
                    .First();
                SokoZaiko sokoZaiko = shiire.ZaikoCreate(shiireMaster, shiireJisseki);
                sokoZaikos.Add(sokoZaiko);
            }

            viewModel.ShiireJisseki = shiireJissekis;
            viewModel.SokoZaiko = sokoZaikos;

            return View("ShiireView", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireView(ShiireViewModel inshiireViewModel) {
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            List<ShiireJisseki> shiireJissekis = inshiireViewModel.ShiireJisseki;
            List<SokoZaiko> sokoZaikos = inshiireViewModel.SokoZaiko;

            List<ShiireJisseki> updatedShiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> updatedSokoZaikos = new List<SokoZaiko>();

            foreach (var (shiireJisseki, sokoZaiko) in shiireJissekis.Zip(sokoZaikos, (a, b) => (a, b))) {
                (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) = shiire.ShiireJissekiUpdate(shiireJisseki, sokoZaiko);
                updatedShiireJissekis.Add(shiireJissekiResult);
                updatedSokoZaikos.Add(sokoZaikoResult);
            }

            foreach (var shiireJisseki in inshiireViewModel.ShiireJisseki) {
                ShiireJisseki shiireJissekii = _context.ShiireJissekis
                    .Where(s => s.ChumonId == shiireJisseki.ChumonId && s.ShiirePrdId == shiireJisseki.ShiirePrdId)
                    .Include(s => s.ChumonJissekiMeisais)
                    .ThenInclude(s => s.ShiireMaster)
                    .ThenInclude(s => s.ShiireSakiMaster)
                    .ThenInclude(s => s.ShiireMasters)
                    .ThenInclude(s => s.ShohinMaster)
                    .First();
            }

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
