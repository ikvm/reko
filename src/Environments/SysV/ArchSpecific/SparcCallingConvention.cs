﻿#region License
/* 
 * Copyright (C) 1999-2017 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using System;
using Reko.Core;
using Reko.Core.Serialization;
using Reko.Core.Types;
using Reko.Core.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace Reko.Environments.SysV.ArchSpecific
{
    public class SparcCallingConvention : CallingConvention
    {
        private RegisterStorage[] regs;
        private RegisterStorage iret;
        private RegisterStorage fret0;
        private RegisterStorage fret1;

        private IProcessorArchitecture arch;

        public SparcCallingConvention(IProcessorArchitecture arch)
        {
            this.arch = arch;
            this.regs = new[] { "o0", "o1", "o2", "o3", "o4", "o5" }
                .Select(r => arch.GetRegister(r))
                .ToArray();
            this.iret = arch.GetRegister("o0");
            this.fret0 = arch.GetRegister("f0");
            this.fret1 = arch.GetRegister("f1");
        }

        public override CallingConventionResult Generate(DataType dtRet, DataType dtThis, List<DataType> dtParams)
        {
            var ccr = new CallingConventionResult();
            ccr.LowLevelDetails(arch.WordWidth.Size, 0x0018);
            if (dtRet != null)
            {
                ccr.Return = this.GetReturnRegister(dtRet);
            }

            int ir = 0;
            for (int iArg = 0; iArg < dtParams.Count; ++iArg)
            {
                var dtArg = dtParams[iArg];
                var prim = dtArg as PrimitiveType;
                if (dtArg.Size <= 8)
                {
                    if (ir >= regs.Length)
                    {
                        ccr.StackParam(dtArg);
                    }
                    else
                    {
                        ccr.RegParam(regs[ir]);
                        ++ir;
                    }
                }
                else
                    throw new NotImplementedException();
            }
            return ccr;
        }

        public Storage GetReturnRegister(DataType dtArg)
        {
            var ptArg = dtArg as PrimitiveType;
            if (ptArg != null)
            {
                if (ptArg.Domain == Domain.Real)
                {
                    var f0 = fret0;
                    if (ptArg.Size <= 4)
                        return f0;
                    var f1 = fret1;
                    return new SequenceStorage(f1, f0);
                }
                return iret;
            }
            else if (dtArg is Pointer)
            {
                return iret;
            }
            else if (dtArg.Size <= this.arch.WordWidth.Size)
            {
                return iret;
            }
            throw new NotImplementedException();
        }
    }
}