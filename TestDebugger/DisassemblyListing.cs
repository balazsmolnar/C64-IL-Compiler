using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TestDebugger;

class ListingRow
{
    public int Address;
    public byte[] Bytes;
    public string SourceText;
    public bool IsData;
}

// Parses 64tass's --list output (prg/dump_debug.asm, already produced as a
// side effect of every debug build -- see TestCompiler.DumpListingPath)
// into an address-indexed structure. This IS the "rich disassembly" --
// real, post-macro-expansion 6502 with symbolic operands and original
// inline comments already baked in by 64tass. No opcode decoding happens
// here, only text parsing of an already-generated file.
//
// Line shapes (confirmed by direct inspection of a real listing file):
//   .1000\t\t\tRun_Test:                 -- label-only row (empty hex)
//   .1000\ta9 06\t\tlda #$06             -- real instruction row
//   .1023\t20 19 3f\tjsr ArrayTest_x_cctor
//   >18ff\t01 02 04 08\tsprite_bit_table: .byte $01, ...  -- data row
//   =$20\t\t\tzp_tmp1_low = $20          -- equate, not an address at all
//   ; comment / header lines, and blank lines
// A "." address can appear twice (a label-only row immediately followed by
// its real instruction row at the same address) -- label text and
// instruction data are indexed separately, not as one row per address.
class DisassemblyListing
{
    private readonly Dictionary<int, List<string>> _labelsByAddress = new();
    private readonly List<ListingRow> _rows = new();
    private readonly Dictionary<int, int> _addressToRowIndex = new();

    public int RowCount => _rows.Count;

    public static DisassemblyListing Load(string listingPath)
    {
        var result = new DisassemblyListing();
        foreach (var rawLine in File.ReadLines(listingPath))
        {
            if (rawLine.Length == 0)
                continue;
            var prefix = rawLine[0];
            if (prefix != '.' && prefix != '>')
                continue; // "=" equate, ";" comment -- not a real address row

            var parts = rawLine.Split('\t');
            if (!int.TryParse(parts[0].Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var address))
                continue; // defensive -- shouldn't happen for a "." / ">" row

            var hexToken = parts.Length > 1 ? parts[1].Trim() : "";
            var bytes = hexToken.Length == 0
                ? Array.Empty<byte>()
                : hexToken.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Select(t => Convert.ToByte(t, 16)).ToArray();

            // Source text = last non-empty tab-separated field, if any --
            // robust against both "empty hex, real source" (label row) and
            // "hex present, no source at all" (data-overflow row) shapes.
            var sourceText = "";
            for (int i = parts.Length - 1; i >= 2; i--)
            {
                if (parts[i].Length > 0) { sourceText = parts[i]; break; }
            }

            if (prefix == '.' && bytes.Length == 0)
            {
                var label = sourceText.TrimEnd(':').Trim();
                if (label.Length > 0)
                {
                    if (!result._labelsByAddress.TryGetValue(address, out var list))
                        result._labelsByAddress[address] = list = new List<string>();
                    list.Add(label);
                }
                continue;
            }

            if (bytes.Length == 0)
                continue; // ">" row with no bytes and no source -- nothing to index

            result._addressToRowIndex[address] = result._rows.Count;
            result._rows.Add(new ListingRow
            {
                Address = address,
                Bytes = bytes,
                SourceText = sourceText,
                IsData = prefix == '>',
            });
        }
        return result;
    }

    public ListingRow TryGetRowAt(int address) =>
        _addressToRowIndex.TryGetValue(address, out var i) ? _rows[i] : null;

    public IReadOnlyList<string> LabelsAt(int address) =>
        _labelsByAddress.TryGetValue(address, out var list) ? list : Array.Empty<string>();

    public ListingRow RowAt(int index) => _rows[index];

    // Index of the row at, or nearest-before, `address`. Returns -1 only if
    // `address` is before every row in the whole listing.
    public int IndexOfRowAtOrBefore(int address)
    {
        int lo = 0, hi = _rows.Count - 1, best = -1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_rows[mid].Address <= address) { best = mid; lo = mid + 1; }
            else hi = mid - 1;
        }
        return best;
    }
}
