using Convenience.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Convenience.Models.ViewModels.Shiire;
using Microsoft.EntityFrameworkCore;
using Convenience.Models.Interfaces;
using Convenience.Servises;
using Convenience.Models.Date.Shiire;
using Convenience.Models.Date.Chumon;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using NuGet.Protocol.Plugins;

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
                .OrderByDescending(c => c.ChumonId)
                .Select(c => new SelectListItem { Value = c.ChumonId, Text = c.ChumonId })
                .ToListAsync()
            };

            ShiireKeyViewModel viewModel = shiireKeyViewModel;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireKey(string inChumonId) {
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            // 仕入実績と在庫を格納するリストを初期化
            List<ShiireJisseki> shiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> sokoZaikos = new List<SokoZaiko>();

            // 注文実績明細を取得
            List<ChumonJissekiMeisai> chumonJissekiMeisais =　await _context.ChumonJissekiMeisais
                .Where(c => c.ChumonId == inChumonId)
                .Include(c => c.ChumonJisseki)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShiireSakiMaster)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShohinMaster)
                .OrderBy(c => c.ShiirePrdId)
                .ToListAsync();

            foreach (var chumonJissekiMeisai in chumonJissekiMeisais) {
                // 注文に対応する仕入実績を取得、作成する
                ShiireJisseki shiireJisseki = shiire.ShiireCreate(chumonJissekiMeisai);
                // リストに仕入実績を追加
                shiireJissekis.Add(shiireJisseki);

                // 仕入マスタを検索して在庫を作成
                ShiireMaster shiireMaster = _context.ShiireMasters
                    .Where(s => s.ShiireSakiId == chumonJissekiMeisai.ShiireSakiId && s.ShiirePrdId == chumonJissekiMeisai.ShiirePrdId && s.ShohinId == chumonJissekiMeisai.ShohinId)
                    .Include(s => s.ShohinMaster)
                    .Include(s => s.ShiireSakiMaster)
                    .First();
                SokoZaiko sokoZaiko = shiire.ZaikoCreate(shiireMaster, shiireJisseki);
                // リストに在庫を追加
                sokoZaikos.Add(sokoZaiko);
            }

            // ViewModelに仕入実績と在庫をセット
            ShiireViewModel viewModel = new ShiireViewModel() {
                ShiireJisseki = shiireJissekis,
                SokoZaiko = sokoZaikos
            };
            return View("ShiireView", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireView(ShiireViewModel inshiireViewModel) {
            string message = null;
            IShiire shiire = new ShiireService(_context);
            ModelState.Clear();

            // 更新された仕入実績と在庫を格納するリストを初期化
            List<ShiireJisseki> updatedShiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> updatedSokoZaikos = new List<SokoZaiko>();

            // 仕入実績と在庫を一緒に処理
            foreach (var (shiireJisseki, sokoZaiko) in inshiireViewModel.ShiireJisseki.Zip(inshiireViewModel.SokoZaiko, (a, b) => (a, b))) {
                if (shiireJisseki.ChumonJissekiMeisais.ChumonSu >= sokoZaiko.SokoZaikoSu) {
                    // 仕入実績と在庫を更新
                    (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) = shiire.ShiireJissekiUpdate(shiireJisseki, sokoZaiko);
                    // 更新された仕入実績と在庫をリストに追加
                    updatedShiireJissekis.Add(shiireJissekiResult);
                    updatedSokoZaikos.Add(sokoZaikoResult);
                    message = "仕入登録完了";
                }
                else {
                    updatedShiireJissekis.Add(shiireJisseki);
                    updatedSokoZaikos.Add(sokoZaiko);
                    message = "エラー：" + shiireJisseki.ChumonJissekiMeisais.ShiirePrdId + "の納入数が注文数を超過";
                }
            }
            // データベースに変更を保存
            await _context.SaveChangesAsync();

            // 更新後の仕入実績と在庫をViewModelに設定
            ShiireViewModel viewModel = new ShiireViewModel() {
                ShiireJisseki = updatedShiireJissekis,
                SokoZaiko = updatedSokoZaikos,
                Remark = message
            };
            // ShiireViewにViewModelを渡してビューを返す
            return View(viewModel);
        }
    }
}
