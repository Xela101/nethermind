//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Nethermind.Abi;
using Nethermind.Baseline.Test.Contracts;
using Nethermind.Baseline.Tree;
using Nethermind.Blockchain.Processing;
using Nethermind.Consensus.AuRa.Contracts;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Core.Test.Builders;
using Nethermind.Db;
using Nethermind.Evm;
using Nethermind.Int256;
using Nethermind.JsonRpc.Test.Modules;
using Nethermind.Logging;
using Nethermind.Specs;
using Nethermind.Specs.Forks;
using Nethermind.Trie;
using NSubstitute;
using NUnit.Framework;

namespace Nethermind.Baseline.Test
{
    public class BaselineTreeTrackerTests
    {
        private IFileSystem _fileSystem;
        private AbiEncoder _abiEncoder;

        [SetUp]
        public void SetUp()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            const string expectedFilePath = "contracts/MerkleTreeSHA.bin";
            _fileSystem.File.ReadAllLinesAsync(expectedFilePath).Returns(File.ReadAllLines(expectedFilePath));
            _abiEncoder = new AbiEncoder();
        }



        [Test]
        public async Task Tree_tracker_insert_leaf([ValueSource(nameof(InsertLeafTestCases))]InsertLeafTest test)
        {
            var address = TestItem.Addresses[0];
            var result = await InitializeTestRpc(address);
            var testRpc = result.TestRpc;
            var baselineModule = result.BaselineModule;
            BaselineTree baselineTree = BuildATree();
            var fromContractAdress = ContractAddress.From(address, 0L);
            var baselineTreeHelper = new BaselineTreeHelper(testRpc.LogFinder);
            new BaselineTreeTracker(fromContractAdress, baselineTree, testRpc.BlockProcessor, baselineTreeHelper);

            var contract = new MerkleTreeSHAContract(_abiEncoder, fromContractAdress);
            UInt256 nonce = 2L;
            for (int i = 0; i < test.ExpectedTreeCounts.Length; i++)
            {
                for (int j = 0; j < test.LeavesInTransactionsAndBlocks[i].Length; j++)
                {
                    var txHash = test.LeavesInTransactionsAndBlocks[i][j];
                    var transaction = contract.InsertLeaf(address, txHash);
                    //transaction.Nonce = nonce;
                    //++nonce;
                    await testRpc.TxSender.SendTransaction(transaction, TxPool.TxHandlingOptions.ManagedNonce);
                    //await baselineModule.baseline_insertLeaf(address, fromContractAdress, txHash);
                    //var insertLeafReceipt = (await testRpc.EthModule.eth_getTransactionReceipt(txHash)).Data;
                    //insertLeafReceipt.Logs.Should().HaveCount(1);
                }

                await testRpc.AddBlock();
                Assert.AreEqual(test.ExpectedTreeCounts[i], baselineTree.Count);
            }
        }

        [Test]
        public async Task Tree_tracker_insert_leaves()
        {
            var address = TestItem.Addresses[0];
            var result = await InitializeTestRpc(address);
            var testRpc = result.TestRpc;
            var baselineModule = result.BaselineModule;
            BaselineTree baselineTree = BuildATree();
            var fromContractAdress = ContractAddress.From(address, 0);
            var baselineTreeHelper = new BaselineTreeHelper(testRpc.LogFinder);
            new BaselineTreeTracker(fromContractAdress, baselineTree, testRpc.BlockProcessor, baselineTreeHelper);

            var contract = new MerkleTreeSHAContract(_abiEncoder, fromContractAdress);
            var hashes = new Keccak[]
            {
                TestItem.KeccakA, TestItem.KeccakB,  TestItem.KeccakC
            };

            var transaction = contract.InsertLeaves(address, hashes);
            await testRpc.TxSender.SendTransaction(transaction, TxPool.TxHandlingOptions.ManagedNonce);

            await testRpc.AddBlock(transaction);
            Assert.AreEqual(3, baselineTree.Count);
        }

        private async Task<(TestRpcBlockchain TestRpc, BaselineModule BaselineModule)> InitializeTestRpc(Address address)
        {
            var spec = new SingleReleaseSpecProvider(ConstantinopleFix.Instance, 1);
            TestRpcBlockchain testRpc = await TestRpcBlockchain.ForTest(SealEngineType.NethDev).Build(spec);
            testRpc.TestWallet.UnlockAccount(address, new SecureString());
            await testRpc.AddFunds(address, 10.Ether());

            BaselineModule baselineModule = new BaselineModule(
                testRpc.TxSender,
                testRpc.StateReader,
                testRpc.LogFinder,
                testRpc.BlockTree,
                new AbiEncoder(),
                _fileSystem,
                new MemDb(),
                LimboLogs.Instance,
                testRpc.BlockProcessor);
            Keccak txHash = (await baselineModule.baseline_deploy(address, "MerkleTreeSHA")).Data;
            await testRpc.AddBlock();
            return (testRpc, baselineModule);
        }

        private BaselineTree BuildATree(IKeyValueStore keyValueStore = null)
        {
            return new ShaBaselineTree(keyValueStore ?? new MemDb(), new byte[] { }, 0);
        }


        public class InsertLeafTest
        {
            // first dimensions - blocks, second dimensions - transactions
            public Keccak[][] LeavesInTransactionsAndBlocks { get; set; }
            public int[] ExpectedTreeCounts { get; set; }

            public override string ToString() => "Tree counts: " + string.Join("; ", ExpectedTreeCounts.Select(x => x.ToString()));
        }


        public static IEnumerable<InsertLeafTest> InsertLeafTestCases
        {
            get
            {
                yield return new InsertLeafTest()
                {
                    LeavesInTransactionsAndBlocks = new Keccak[][]
                    {
                        new Keccak[] // first block
                        {
                            TestItem.KeccakB // first transaction
                        }
                    },
                    ExpectedTreeCounts = new int[]
                    {
                        1 // tree count after first block
                    }
                };

                yield return new InsertLeafTest()
                {
                    LeavesInTransactionsAndBlocks = new Keccak[][]
                    {
                        new Keccak[] // first block
                        {
                            TestItem.KeccakB, // first transaction
                            TestItem.KeccakC // second transaction 
                        }
                    },
                    ExpectedTreeCounts = new int[]
                    {
                        2 // tree count after first block
                    }
                };

                yield return new InsertLeafTest()
                {
                    LeavesInTransactionsAndBlocks = new Keccak[][]
                    {
                        new Keccak[] // first block
                        {
                            TestItem.KeccakA, // first transaction
                            TestItem.KeccakC, // second transaction 
                            TestItem.KeccakD, // third transaction 
                            TestItem.KeccakF
                        },
                        new Keccak[] // second block
                        {
                            TestItem.KeccakB, // first transaction
                            TestItem.KeccakC // second transaction,
                        }
                    },
                    ExpectedTreeCounts = new int[]
                    {
                        3, // tree count after first block
                        5 // tree count after second block
                    }
                };
            }
        }
    }
}
