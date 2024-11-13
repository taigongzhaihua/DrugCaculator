import requests
from lxml import etree


base_url = 'https://drugs.dxy.cn/pc/search?keyword='


class bbs_genspider(object):
    def __init__(self, drugName):
        self.url = base_url + drugName + '&type=drug&querySpellCheck=true'
    def get_html(self):
        headers = {
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7',
            'Accept-Encoding': 'gzip, deflate',
            'Accept-Language': 'zh-CN,zh;q=0.9',
            'Cache-Control': 'max-age=0',
            'Connection': 'keep-alive',
            'Cookie': 'dxy_da_cookie-id=0111b3131f43681b403dc9705e149dff1731331999913; Hm_lvt_5fee00bcc4c092fe5331cc51446d8be2=1731471162; HMACCOUNT=023855ED8E3B5B7D; _gid=GA1.2.1302475338.1731473452; _ga_C1KRQPKH2N=GS1.2.1731473451.1.0.1731473452.0.0.0; CLASS_CASTGC=51d9660dffbc7edba7d44b463fd9885314489f156e63d04a33a8a7d6e6ffcdee8c42e5dd432e1d2c996dd304a8fea8a2ee275ae7544e23a98722c21c6072379c61fd72dae12090ef73b35f0d9098f78404f1a8b33e908196a494512c6b012bc448b45cedfd744422cf71fcab1983c611ad220bd3792a390da0333c8ac96810a60482c0279aa4be1179d28500462de3339f130730789dd6571410457a757536d02d9f92140c50b5319b0334077d707eafcb3a72407d29f52332c7487304cf76bbbc33e306c32c77ee352b0e70b5b3c89c7d63f6d34eceee5c04d91f0855b3a3628a70cc6d8852edf992421852307ceb80708571a0ca4d36ac4b35b6684a1ae0ea; PHP_COMMON_CASTGC=53e8e7a3d63c7103d7a883cb27a6ce25fc56d395df2d9b76bdd76ce69e7d496079e8c7645be8704a1f4ea57c16ecf40813843341e7751ce8ecdb06f259a180eb8f51b7aae53b0daa864e3b44197246d26ceeb2293d7c58262359f6becee418596036a04b208f6fd519856fdbee9952020dfbf4706c3460d65584b1d9c952fd82e28a96f03a47ccc34c4085ae6014b13c2a84bc8b53b04829cdf1bd70cdb547d282181ef73fcb12c2025ec177abca9e5ae0a1d25dd4ff79700b0fb58d60691a5ec1826ae15d117e95f1f8075a3be3833dba9c19bc6d9716fd030fc9c1fc4214865ccedb50b94f27fd917e655a5860ae71957bbf7fad1dc74ea7fdbdcc26522d36; JUTE_BBS_DATA=75533e855fc07806d8a4b948dc06c136273a3e587d98928920ef151de160c614d75b1e3a776998cabccbf43f7261e4dd3d679cd09df2532a6e4333dc19ba68ae5ca4872f1495c9f1409785664df7f0e70a3cbbb88c353beca1424cbc65f0faa9aae1dfb9a200dcb688144d356771adbf; _ga=GA1.1.363651723.1731473413; _ga_LTBPLJJK75=GS1.1.1731473413.1.1.1731473893.0.0.0; Hm_lpvt_5fee00bcc4c092fe5331cc51446d8be2=1731473895',
            'Host': 'www.dxy.cn',
            'Referer': 'https://auth.dxy.cn/',
            'Upgrade-Insecure-Requests': '1',
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0',
        }
        req = requests.get(self.url, headers=headers).text
        return req



raw_id = '12345'
bbs = bbs_genspider(raw_id)
bbs_id,bbs_avater = bbs.del_common(raw_id)
print("----------------------------------")
print(bbs_id)
print(len(bbs_id))
print(bbs_avater)
print(len(bbs_avater))