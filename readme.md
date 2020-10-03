# COVID19 Auto Diagnostic - 새 자가진단 시스템

* 새 자가진단 시스템이 나왔음에도 불구하고 여전히 프로그램은 작동합니다.
* Release1-NOT WORKING 폴더는 더이상 작동되지 않는 지자체 도메인 기반으로 제작된 프로그램 프로젝트 폴더입니다.
따라서 그 속에 readme.md, mockup.py가 있습니다.

# 새 자가진단 시스템 정보 

서버 정보 상에는 경기도 서울에 위치해있습니다. 하지만, 아이피에 따른 위치를 검색해보니 '대구광역시 수성구'이었습니다. https 포트는 443을 사용합니다.

# Workflow 워크플로

loginwithschool -> secondlogin -> https://gbehcs.eduro.go.kr/registerServey

성공한 secondlogin 중 OPTIONS 메소드의 WAF 쿠키값은 registerServey에서 WAF 쿠키값과 일치합니다.  
++ loginwithschool POST WAF 쿠키와 registerServey POST WAF 쿠키가 일치합니다.

실패시 실패 카운트가 증가하는데,  
실패 카운트가 5가 되면 5분 정지입니다. 

로그인 후에 https://hcs.eduro.go.kr/#/main 에선 클래스 memberWrap인 selection 태그의 두번째부터 참여자 목록이 보입니다. 첫 자가진단 시간을 확인 할 수 있습니다. 


### 단순 워크플로
학교 정보 입수 -> 비밀번호 입력 (혹은 초기 설정) -> 자가진단 -> 완료


# https://hcs.eduro.go.kr/school - 학교 정보 입수
위 주소는 학교의 상세정보를 얻는데 씁니다. 

GET 형식으로 작동합니다. 


-----------------------
## Response

학교의 일부 단어만 입력하여도 됩니다.
Response 데이터는 schulList 오브젝트에 차곡차곡 관련 학교데이터가 저장됩니다   
배열 형식으로 학교들이 저장되며, 각각엔  

orgCode : 학교 코드입니다.  
kraOrgNm : 학교 이름입니다.  
addres : 학교 주소입니다.  
atptOfcdcConctUrl : 적합한 도 코드가 들어간 도메인입니다. (= 지역코드hcs.eduro.go.kr)

이러한 프로퍼티들이 있습니다.

그 외에도 많은 정보가 schulList 오브젝트에 담겨있습니다.  
따라서, 사용자의 선택 학교의 orgCode를 가져와 사용할 수 있습니다.

## Request
```
https://hcs.eduro.go.kr/school?lctnScCode=지역코드&schulCrseScCode=학교급&orgName=학교이름&currentPageNo=1
```
각각의 한글 이름들은 '숫자'로 대응됩니다. (학교 이름 제외)

## 지역코드 (위 링크 매개변수에 한한 코드)
* 01부터 18까지 있습니다 **주의 :09번 지역코드는 없습니다.**  


서울특별시 = 01  
부산광역시 = 02  
대구광역시 = 03  
인천광역시 = 04  
광주광역시 = 05  
대전광역시 = 06  
울산광역시 = 07  
세종특별자치시 = 08  
경기도 = 10  
강원도 = 11  
충청북도 = 12  
충청남도 = 13  
전라북도 = 14  
전라남도 = 15  
경상북도 = 16  
경상남도 = 17  
제주특별자치도 = 18  

## 학교급
---------
유치원 = 1  
초등학교 = 2  
중학교 = 3  
고등학교 = 4  
특수학교 = 5  

## 학교 이름
-----------
학교이름은 재학중인 학교의 이름을 쓰시면 됩니다. 

# https://지역코드hcs.eduro.go.kr/loginwithschool - 학교 로그인

이름, 생년월일, 학교코드(ex r001)로 로그인합니다.  
POST 메소드 입니다

## Request

```{   
  "orgcode":"학교 코드",  
  "name":"암호화된 이름",  
  "birthday":"암호화된 생년월일(6자리 형식)"   
}
```

## Response

```
{ "registerDtm":"가입일 Date Time","admnYn":"어드민 여부 Y/N",  "orgname":"학교이름","registerYmd":"가입일 Year Month Day","mngrClassYn":"N",  "name":"이름","man":"N","stdntYn":"학생 여부 Y/N",  "infAgrmYn":"Y","token":"배리어 토큰","mngrDeptYn":"N","isHealthy":건강 여부 boolean형 }  
```

# https://지역코드hcs.eduro.go.kr/secondlogin - 비밀번호 로그인

암호화된 비밀번호를 전송합니다 POST 메소드입니다.

## Request 

```
{ "password":"암호화된 비밀번호","deviceUuid":""}
```

## Response 

1. 성공
  ```
  {"sndLogin":{"existsYn":"Y","validYn":"Y"}}
  ```

2. 실패
  ```
  {"isError":true,"statusCode":252,"errorCode":1001,"data":{"failCnt":실패 카운트,"canInitPassword":false}}
  ```

# https://지역코드hcs.eduro.go.kr/registerServey - 설문지 제출

## Cookies

secondlogin때 WAF 쿠키값과 일치하는 값을 가집니다. 
쿠키 유지 방안 (ex Python requests.session) 필요할듯 보입니다.

## Request
```
{"rspns01":"1","rspns02":"1","rspns03":null,"rspns04":null,"rspns05":null,"rspns06":null,"rspns07":"0","rspns08":"0","rspns09":"0","rspns10":null,"rspns11":null,"rspns12":null,"rspns13":null,"rspns14":null,"rspns15":null,"rspns00":"Y","deviceUuid":""}
```
설문지 내용에 따라 저 값들은 달라지며, 체크박스 2개 이상인 경우, 최솟값(= 긍정)은 1이며, 2개중 한개를 택하는 설문은 긍정값(= 아니요)은 0입니다.

* 그전과는 다르게 HTML내에서 어느 질문이 rspns.. 인지 알 수가 없습니다.

## Response

```
{"registerDtm":"Sep 10, 2020 9:54:58 PM","inveYmd":"20200910"}
```

자가진단 날짜가 나옵니다.

# 결과

자가진단이 끝난 후, HTML이 자동 갱신되는데 이때  
https://hcs.eduro.go.kr/#/survey 의 
DIV태그 중, 클래스가 guid_contents type2인 것의 p태그의 innerText가 최종 텍스트 입니다.  
예 : 코로나19 예방을 위한 자가진단 설문결과 의심 증상에 해당되는 항목이 없어 등교가 가능함을 안내드립니다.


# 도메인 지역코드

1. 서울 sen
2. 전라북도 jbe
3. 전라남도 jne
4. 경기도 goe
5. 경상북도 gbe
6. 경상남도 gne
7. 충청북도 cbe
8. 충청남도 cne
9. 대구광역시 dge
10. 부산광역시 pen
11. 제주특별자치도 jje
12. 세종특별자치시 sje
13. 광주광역시 gen
14. 인천광역시 ice
15. 대전광역시 dje
16. 울산광역시 use
17. 강원도 gwe
