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
using System.Collections.Generic;
using Reko.Core;
using Reko.Core.Types;
using Reko.Arch.X86;
using System.Diagnostics;

namespace Reko.Environments.Windows
{
    /// <summary>
    /// Windows calling convention for x86-64.
    /// </summary>
    public class X86_64CallingConvention : CallingConvention
    {
        private static readonly RegisterStorage[] iRegs =
        {
            Registers.rcx,
            Registers.rdx,
            Registers.r8,
            Registers.r9,
        };

        private static readonly RegisterStorage[] fRegs =
        {
            Registers.xmm0,
            Registers.xmm1,
            Registers.xmm2,
            Registers.xmm3,
        };

        public override CallingConventionResult Generate(DataType dtRet, DataType dtThis, List<DataType> dtParams)
        {
            var ccr = new CallingConventionResult();
            ccr.LowLevelDetails(8, 0x0028);
            if (dtRet != null)
            {
                if (dtRet.Size > 8)
                    throw new NotImplementedException();
                var pt = dtRet as PrimitiveType;
                if (pt != null && pt.Domain == Domain.Real)
                {
                    ccr.Return = Registers.xmm0;
                }
                else if (dtRet.Size <= 1)
                {
                    ccr.Return = Registers.al;
                }
                else if (dtRet.Size <= 2)
                {
                    ccr.Return = Registers.ax;
                }
                else if (dtRet.Size <= 4)
                {
                    ccr.Return = Registers.eax;
                }
                else
                { 
                    ccr.Return = Registers.rax;
                }
            }
            for (int i = 0; i < dtParams.Count; ++i)
            {
                var dt = dtParams[i];
                if (dt.Size > 8)
                    throw new NotImplementedException();
                var pt = dt as PrimitiveType;
                if (pt != null && pt.Domain == Domain.Real && i < fRegs.Length)
                {
                    ccr.RegParam(fRegs[i]);
                }
                else if (i < iRegs.Length)
                {
                    ccr.RegParam(iRegs[i]);
                }
                else
                {
                    ccr.StackParam(dt);
                }
            }
            ccr.StackDelta = 8;
            return ccr;
        }
    }
}