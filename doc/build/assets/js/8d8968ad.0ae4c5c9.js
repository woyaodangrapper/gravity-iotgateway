"use strict";(self.webpackChunkthingsgateway=self.webpackChunkthingsgateway||[]).push([[8976],{137:(t,e,a)=>{a.r(e),a.d(e,{assets:()=>i,contentTitle:()=>o,default:()=>k,frontMatter:()=>r,metadata:()=>p,toc:()=>u});var n=a(7462),l=(a(7294),a(3905));a(4996),a(510),a(2969);const r={id:20005,title:"Dlt645"},o=void 0,p={unversionedId:"20005",id:"20005",title:"Dlt645",description:"\u5b9a\u4e49",source:"@site/docs/20005.mdx",sourceDirName:".",slug:"/20005",permalink:"/thingsgateway-docs/docs/20005",draft:!1,editUrl:"https://gitee.com/diego2098/ThingsGateway/tree/master/doc/docs/20005.mdx",tags:[],version:"current",lastUpdatedBy:"Kimdiego2098",lastUpdatedAt:1706236999,formattedLastUpdatedAt:"Jan 26, 2024",frontMatter:{id:"20005",title:"Dlt645"},sidebar:"docs",previous:{title:"OpcUa",permalink:"/thingsgateway-docs/docs/20004"},next:{title:"\u8054\u7cfb\u6211\u4eec",permalink:"/thingsgateway-docs/docs/1002"}},i={},u=[{value:"\u5b9a\u4e49",id:"\u5b9a\u4e49",level:2},{value:"\u4e00\u3001\u8bf4\u660e",id:"\u4e00\u8bf4\u660e",level:2},{value:"\u4e8c\u3001Dlt645-2007\u4e3b\u7ad9",id:"\u4e8cdlt645-2007\u4e3b\u7ad9",level:2}],d={toc:u},s="wrapper";function k(t){let{components:e,...a}=t;return(0,l.kt)(s,(0,n.Z)({},d,a,{components:e,mdxType:"MDXLayout"}),(0,l.kt)("h2",{id:"\u5b9a\u4e49"},"\u5b9a\u4e49"),(0,l.kt)("p",null,"\u7a0b\u5e8f\u96c6\uff1a",(0,l.kt)("a",{parentName:"p",href:"https://www.nuget.org/packages/ThingsGateway.Foundation.Dlt645"},"ThingsGateway.Foundation.Dlt645")),(0,l.kt)("h2",{id:"\u4e00\u8bf4\u660e"},"\u4e00\u3001\u8bf4\u660e"),(0,l.kt)("p",null,(0,l.kt)("strong",{parentName:"p"},"ThingsGateway.Foundation.Dlt645"),"\u662f\u5bf9\u4e8eDlt645-2007\u534f\u8bae\u7684\u5c01\u88c5\u7c7b\u5e93"),(0,l.kt)("p",null,"\u652f\u6301\u591a\u4e2a\u901a\u8baf\u94fe\u8def\uff1aTcp/Udp/SerialPort"),(0,l.kt)("h2",{id:"\u4e8cdlt645-2007\u4e3b\u7ad9"},"\u4e8c\u3001Dlt645-2007\u4e3b\u7ad9"),(0,l.kt)("p",null,"1\u3001\u521b\u5efaDlt645Master"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},'        /// <summary>\n        /// \u65b0\u5efa\u94fe\u8def\n        /// </summary>\n        /// <returns></returns>\n        public IChannel GetChannel()\n        {\n            TouchSocketConfig touchSocketConfig = new TouchSocketConfig();\n            return touchSocketConfig.GetSerialPortWithOption(new("COM1")); //\u76f4\u63a5\u83b7\u53d6\u4e32\u53e3\u5bf9\u8c61\n            //return touchSocketConfig.GetChannel(ChannelTypeEnum.SerialPortClient, null, null, new("COM1"));//\u901a\u8fc7\u94fe\u8def\u679a\u4e3e\u83b7\u53d6\u5bf9\u8c61\n        }\n\n        /// <summary>\n        /// \u65b0\u5efa\u534f\u8bae\u5bf9\u8c61\n        /// </summary>\n        /// <param name="channel"></param>\n        /// <returns></returns>\n        public IProtocol GetProtocol(IChannel channel)\n        {\n            var client = new Dlt645_2007Master(channel);\n            client.Station = "311111111114";//\u8868\u53f7\n            return client;\n        }\n')),(0,l.kt)("p",null,"2\u3001\u8bfb\u5199\u64cd\u4f5c"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},'            Dlt645MasterTest dlt645MasterTest = new Dlt645MasterTest();\n            var channel = dlt645MasterTest.GetChannel();\n            var protocol = dlt645MasterTest.GetProtocol(channel);\n            var data = await protocol.ReadDoubleAsync("02010100"); //\u8bfb\u53d6A\u76f8\u7535\u538b\n\n')),(0,l.kt)("p",null,(0,l.kt)("inlineCode",{parentName:"p"},"02010100"),"\u662fDlt645\u4e2d\u7684\u5730\u5740\u8868\u793a\u65b9\u5f0f\uff0c\u4ee3\u8868A\u76f8\u7535\u538b\uff0c\u8bf7\u67e5\u770b\u76f8\u5173\u534f\u8bae\u6587\u6863\uff0c\u53ef\u5728\u6e90\u7801\u9644\u4ef6\u4e2d\u627e\u5230\u6587\u6863"),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},"\u57fa\u672c\u5730\u5740")),(0,l.kt)("table",null,(0,l.kt)("thead",{parentName:"table"},(0,l.kt)("tr",{parentName:"thead"},(0,l.kt)("th",{parentName:"tr",align:null},"\u5730\u5740"),(0,l.kt)("th",{parentName:"tr",align:null},"\u8bf4\u660e"))),(0,l.kt)("tbody",{parentName:"table"},(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"02010100"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8bfb\u53d602010100(A\u76f8\u7535\u538b)")))),(0,l.kt)("p",null,"\u53c2\u8003DLT2007\u534f\u8bae\u6587\u6863\u6570\u636e\u6807\u8bc6\uff0c\u53ef\u5728\u63d2\u4ef6\u6e90\u7801\u4e2d\u627e\u5230\u9644\u4ef6"),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("p",{parentName:"li"},"\u7ad9\u53f7(\u53ef\u9009)"),(0,l.kt)("p",{parentName:"li"},"\u5f53\u9700\u8981\u6307\u5b9a\u7ad9\u53f7\u5730\u5740\u65f6\u53ef\u4f7f\u7528\uff0c\u4e3e\u4f8b\uff1a"))),(0,l.kt)("table",null,(0,l.kt)("thead",{parentName:"table"},(0,l.kt)("tr",{parentName:"thead"},(0,l.kt)("th",{parentName:"tr",align:null},"\u5730\u5740"),(0,l.kt)("th",{parentName:"tr",align:null},"\u8bf4\u660e"))),(0,l.kt)("tbody",{parentName:"table"},(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"s=111111111111;02010100"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8bfb\u53d602010100 ,\u8bbe\u5907\u5730\u5740\u4e3a111111111111")))),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("p",{parentName:"li"},"Dtu\u6ce8\u518c(\u53ef\u9009)"),(0,l.kt)("p",{parentName:"li"},"\u5f53\u9700\u8981\u6307\u5b9aDtu\u5ba2\u6237\u7aef\u53ef\u4f7f\u7528\uff0c\u4e3e\u4f8b\uff1a"))),(0,l.kt)("table",null,(0,l.kt)("thead",{parentName:"table"},(0,l.kt)("tr",{parentName:"thead"},(0,l.kt)("th",{parentName:"tr",align:null},"\u5730\u5740"),(0,l.kt)("th",{parentName:"tr",align:null},"\u8bf4\u660e"))),(0,l.kt)("tbody",{parentName:"table"},(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"id=12;40001"),(0,l.kt)("td",{parentName:"tr",align:null},'\u8bfb\u53d603\u529f\u80fd\u7801 ,\u8bbe\u5907\u5730\u5740\u4e3a\u9ed8\u8ba4\uff0cDtu\u6ce8\u518c\u5305\u4e3a"12",\u6ce8\u610f\u662fUTF8\u683c\u5f0f')))),(0,l.kt)("p",null,"3\u3001\u5176\u4ed6\u65b9\u6cd5"),(0,l.kt)("p",null,"\u4fee\u6539\u5bc6\u7801"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},"      var result = await protocol.WritePasswordAsync(level, oldPassword, newPassword);\n")),(0,l.kt)("p",null,"\u66f4\u6539\u8868\u53f7"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},"      var result = await protocol.WriteDeviceStationAsync(station);\n")),(0,l.kt)("p",null,"\u4fee\u6539\u6ce2\u7279\u7387"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},"      var result = await protocol.WriteBaudRateAsync(baudRate);\n")),(0,l.kt)("p",null,"\u8bfb\u53d6\u8868\u53f7"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},"      var result = await protocol.ReadDeviceStationAsync;\n")))}k.isMDXComponent=!0}}]);