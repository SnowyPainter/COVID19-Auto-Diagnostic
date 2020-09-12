import requests, json, pprint
from bs4 import BeautifulSoup
import urllib.parse
print("**e 형식, 남도는 *ne, 북도는 *be, 경기는 goe 등.")
state = input("지역코드 입력해주세요 (ex:경남 gne, 충북 cbe) : ")
host = 'eduro.gbe.kr' #교육청, 학교 api는 따로 신청해야함;;
domain = 'https://'+host

req = requests.session()
res = req.get(domain+"/hcheck/index.jsp")

school = input("학교를 적어주세요 : ")
name = input("이름을 입력해주세요 : ")
birthday = input("주민번호 6자리 입력해주세요 (060405, 06년생 4월 5일) : ")

body = {"schulNm":school}
header = {
    "User-Agent":"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/| 20100101 Firefox/79.0",
    "Content-Type":"application/x-www-form-urlencoded; charset=UTF-8",
}

json = req.post(domain+"/stv_cvd_co00_004.do", data=body, headers=header).json()

if(json["resultSVO"]["rtnRsltCode"] == "SUCCESS"):
    print("학교 정보 받기 성공")

    schulCode = json["resultSVO"]["schulCode"]
    body = {"qstnCrtfcNoEncpt":"","rtnRsltCode":"","schulCode":schulCode,"schulNm":school,"pName":name,"frnoRidno":birthday,"aditCrtfcNo":""}
    json = req.post(domain+"/stv_cvd_co00_012.do", data=body, headers=header).json()
    
    rsltCode = json["resultSVO"]["rtnRsltCode"]

    body["rtnRsltCode"] = rsltCode
    print("========================================")
    if(rsltCode == "SUCCESS"):
        print("개인 정보 교환후 검증 성공")
        qstnCrtfcNoEncpt = json["resultSVO"]["qstnCrtfcNoEncpt"]
        
        body["qstnCrtfcNoEncpt"] = qstnCrtfcNoEncpt
        
        html = req.post(domain+"/stv_cvd_co00_000.do", data=body, headers=header).text
        
        soup = BeautifulSoup(html, 'html.parser')
        form = soup.find("form", {"id":"infoForm"})
        bestAnswers = {}
        titles = []
        for t in form.find_all("table"):
            titles.append(t.find("th", {"scope":"row"}).text)
            input = t.find("label").find("input")
            
            bestAnswers[input["name"]] = input["value"]
        
        body.update(bestAnswers)

        json = req.post(domain+"/stv_cvd_co01_000.do", data=body, headers=header).json()
        if(json["resultSVO"]["rtnRsltCode"] == "SUCCESS"):
            print("설문지 정상 제출 완료")
            html = req.post(domain+"/stv_cvd_co02_000.do", data=body, headers=header).text
            soup = BeautifulSoup(html, 'html.parser')
            resultMsg = soup.find("div", {"id":"content_detail1"}).find("p").text
            print("결과")
            print(resultMsg.strip())
            print("완료되었습니다")
            print("========================================")
    else:
        print("2.개인 정보 검증 실패")

else:
    print("1.학교 정보 받기 실패")
    pprint.pprint(body)
