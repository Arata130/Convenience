using System;
using Convenience.Models.Date.Chumon;

namespace Convenience.Models.Interfaces
{

    public interface IChumon
    {
        public ChumonJisseki chumonJisseki { get; set; }

        public ChumonJisseki ChumonCreate(string inShireSakiId);

        public ChumonJisseki ChumonToiawase(string inShireSakiId, DateOnly inChumonDate);

        public ChumonJisseki ChumonUpdate(ChumonJisseki inChumonJisseki);

        /*
         * 注文実績＋注文明細更新
         *  引数　  注文実績
         *  戻り値　注文実績
         *
         *  ①注文実績の検索（キー注文コード）
         *  ②上記でデータなし→引数で渡された注文実績情報をＤＢに新規登録する
         *  ③上記でデータあり→ＤＢの内容に引数で渡されたデータを上書きする
         *  ④savechangesの実行
         */


    }
}