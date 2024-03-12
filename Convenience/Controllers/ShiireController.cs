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
        public async Task<IActionResult> ShiireKey(string inChumonId, ShiireViewModel viewModel) {
            // 引数から注文IDを取得
            string ChumonId = inChumonId;
            // 仕入サービスのインスタンスを作成
            IShiire shiire = new ShiireService(_context);
            // ModelStateをクリア
            ModelState.Clear();

            // 注文実績明細を取得
            List<ChumonJissekiMeisai> chumonJissekiMeisais = _context.ChumonJissekiMeisais
                .Where(c => c.ChumonId == ChumonId)
                .Include(c => c.ChumonJisseki)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShiireSakiMaster)
                .Include(c => c.ShiireMaster)
                .ThenInclude(c => c.ShohinMaster)
                .Include(c => c.ShiireJisseki)
                .OrderBy(c => c.ShiirePrdId)
                .ToList();

            // 仕入実績と在庫を格納するリストを初期化
            List<ShiireJisseki> shiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> sokoZaikos = new List<SokoZaiko>();

            // 当日の日付を取得
            DateOnly ShiireDate = DateOnly.FromDateTime(DateTime.Today);

            // 注文実績明細ごとに処理
            foreach (var chumonJissekiMeisai in chumonJissekiMeisais) {
                // 注文に対応する仕入実績を取得する
                ShiireJisseki shiireJisseki = shiire.ShiireToiawase(ChumonId, ShiireDate);
                if (shiireJisseki == null) {  // 実績がなかった場合（当日）
                                              // 新しい仕入実績を作成
                    shiireJisseki = shiire.ShiireCreate(chumonJissekiMeisai);
                }
                else {  // 仕入実績があった場合（当日）
                        // 時間のみ変更
                    shiireJisseki.ShiireDateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                }
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
            viewModel.ShiireJisseki = shiireJissekis;
            viewModel.SokoZaiko = sokoZaikos;

            // ShiireViewにViewModelを渡してビューを返す
            return View("ShiireView", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShiireView(ShiireViewModel inshiireViewModel) {
            // 仕入サービスのインスタンスを作成
            IShiire shiire = new ShiireService(_context);
            // ModelStateをクリア
            ModelState.Clear();

            // ViewModelから仕入実績と在庫を取得
            List<ShiireJisseki> shiireJissekis = inshiireViewModel.ShiireJisseki;
            List<SokoZaiko> sokoZaikos = inshiireViewModel.SokoZaiko;

            foreach (var shiireJisseki in inshiireViewModel.ShiireJisseki) {
                shiireJisseki.ShiireDateTime = DateTime.SpecifyKind(shiireJisseki.ShiireDateTime, DateTimeKind.Utc);
            }

            // 更新された仕入実績と在庫を格納するリストを初期化
            List<ShiireJisseki> updatedShiireJissekis = new List<ShiireJisseki>();
            List<SokoZaiko> updatedSokoZaikos = new List<SokoZaiko>();

            // 仕入実績と在庫を一緒に処理
            foreach (var (shiireJisseki, sokoZaiko) in shiireJissekis.Zip(sokoZaikos, (a, b) => (a, b))) {

                ChumonJissekiMeisai chumonJissekiMeisai = _context.ChumonJissekiMeisais.Where(c => c.ChumonId == shiireJisseki.ChumonId && c.ShiirePrdId == shiireJisseki.ShiirePrdId && c.ShohinId == shiireJisseki.ShohinId)
                    .Include(c => c.ShiireMaster)
                    .ThenInclude(c => c.ShiireSakiMaster)
                    .Include(c => c.ShiireMaster)
                    .ThenInclude(c => c.ShohinMaster)
                    .First();

                // 仕入実績と在庫を更新
                (ShiireJisseki shiireJissekiResult, SokoZaiko sokoZaikoResult) = shiire.ShiireJissekiUpdate(shiireJisseki, sokoZaiko);
                // 更新された仕入実績と在庫をリストに追加
                updatedShiireJissekis.Add(shiireJissekiResult);
                updatedSokoZaikos.Add(sokoZaikoResult);
            }

            // データベースに変更を保存
            await _context.SaveChangesAsync();

            // 更新後の仕入実績と在庫をViewModelに設定
            ShiireViewModel viewModel = new ShiireViewModel() {
                ShiireJisseki = updatedShiireJissekis,
                SokoZaiko = updatedSokoZaikos,
                Remark = "仕入登録完了"
            };
            // ShiireViewにViewModelを渡してビューを返す
            return View(viewModel);
        }
    }
}
