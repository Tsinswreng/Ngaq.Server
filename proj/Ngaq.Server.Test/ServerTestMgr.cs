using Ngaq.Core.Test;
using Ngaq.Local.Test;
using Ngaq.Server.Test.Domains.User.Http;
using Ngaq.Server.Test.Domains.User.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Server.Test;

public class ServerTestMgr: DiEtTestMgr{
	public static ServerTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? test){
		test = this.TestNode;
		test.Ordered = true;
		test.IsParallelRecursive = false;
		this.RegisterSubMgr(CoreTestMgr.Inst);
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(ServerBizTestMgr.Inst);
		return test;
	}
}

public class ServerBizTestMgr: DiEtTestMgr{
	public static ServerBizTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? test){
		test = this.TestNode;
		test.Ordered = true;
		test.IsParallelRecursive = false;
		this.RegisterTester<TestISvcUser>();
		this.RegisterTester<TestCtrlrUser>();
		return test;
	}
}
