using System;
using System.Collections.Generic;
using System.Linq;
using Solnet.Programs.Utilities;
using Solnet.Rpc.Models;
using Solnet.Wallet;

namespace Script.Model
{
    public class ValueProgram
    {
        static ValueProgram()
        {
            PROGRAM_ID_DEVNET = new PublicKey("G4YkbRN4nFQGEUg4SXzPsrManWzuk8bNq9JaMhXepnZ6");
            ACCOUNT_PUBLIC_KEY_DEVNET = new PublicKey("4AXy5YYCXpMapaVuzKkz25kVHzrdLDgKN3TiQvtf1Eu8");
            PROGRAM_ID_MAINNET_BETA = new PublicKey("EN2Ln23fzm4qag1mHfx7FDJwDJog5u4SDgqRY256ZgFt");
            ACCOUNT_PUBLIC_KEY_MAINNET_BETA = new PublicKey("EajAHVxAVvf4yNUu37ZEh8QS7Lk5bw9yahTGiTSL1Rwt");
        }
        
        /// <summary>
        /// The offset at which the value which defines the program method begins. 
        /// </summary>
        internal const int MethodOffset = 0; 
        
        private const int SIZE_BYTES = 4;
        
        private const int SIZE_BITS = 1;
        
        public static PublicKey PROGRAM_ID_DEVNET;
        
        public static PublicKey ACCOUNT_PUBLIC_KEY_DEVNET;
        
        private static PublicKey PROGRAM_ID_MAINNET_BETA;
        
        private static PublicKey ACCOUNT_PUBLIC_KEY_MAINNET_BETA;
        
        private const int INSTRUCTION_SET_VALUE = 0;
        
        public static PublicKey ProgramId()
        {
            return PROGRAM_ID_DEVNET;
        }
        
        private static PublicKey AccountPublicKey()
        {
            return  ACCOUNT_PUBLIC_KEY_DEVNET;
        }
        
        // val buffer = BorshBuffer.allocate(Byte.SIZE_BYTES + Int.SIZE_BYTES)
        public static TransactionInstruction CreateSetValaueInstruction(int value, string walletAddress)
        {
            var index = ValueProgram.MethodOffset;
            var data = new byte[ValueProgram.SIZE_BITS + ValueProgram.SIZE_BYTES];
            index += data.WriteU8(Convert.ToByte(ValueProgram.INSTRUCTION_SET_VALUE), index);
            index += data.WriteS32(value, index);
            
            List<AccountMeta> keys = new()
                                     {
                                         AccountMeta.Writable(AccountPublicKey(), false),
                                         AccountMeta.ReadOnly(new PublicKey(walletAddress), true),
                                     };
        
            var tmp = ValueProgram.ProgramId().KeyBytes.Select(b => (sbyte)b).ToArray();
            var sData = data.Select(b => (sbyte)b).ToArray();
            return new TransactionInstruction
                   {
                       ProgramId = ProgramId().KeyBytes,
                       Keys = keys,
                       Data = data
                   };
        }
    }
}