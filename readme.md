# COVID19 Auto Diagnostic - 새 자가진단 시스템

* Release1-NOT WORKING 폴더는 더이상 작동되지 않는 지자체 도메인 기반으로 제작된 프로그램 프로젝트 폴더입니다.

교육부가 다른 기능을 추가한다고 이름이 조금 바뀌고 상위 url 'v2'의 하위 url로 옮겨졌습니다. 

암호화 무작위 salt를 하는것 같습니다.

## 지역코드 (위 링크 매개변수에 한한 코드)
------------------
<details><summary>지역코드 리스트</summary>
<p>
01부터 18까지 있습니다 **주의 :09번 지역코드는 없습니다.**  

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
</p>
</details>

-------

## 학교급
---------
<details><summary>학교급 리스트</summary>
<p>
유치원 = 1  
초등학교 = 2  
중학교 = 3  
고등학교 = 4  
특수학교 = 5  
</p>
</details>

----------

# Workflow 워크플로

searchSchool -> findUser -> hasPassword -> validatePassword -> (selectUserGroup -> getUserInfo)

validatePassword에서 비밀번호 검사를 합니다.
실패시 실패 카운트(failCnt)가 증가하는데,  
실패 카운트가 5가 되면 5분 정지입니다. 

(selectUserGroup -> getUserInfo)에서는 자가진단 참여자 목록을 얻고 그 개개인의 정보를 얻습니다. (ex 출석 여부)  

매번 token을 response 하는데, Request Header에 Authorization에 넣어줘야합니다.

# https://hcs.eduro.go.kr/v2/searchSchool - 학교 정보 입수
위 주소는 학교의 상세정보를 얻는데 씁니다. 

GET 형식으로 작동합니다. 

-----------------------
## Request
```
https://hcs.eduro.go.kr/v2/searchSchool?lctnScCode=지역코드&schulCrseScCode=학교급&orgName=학교이름&loginType=school
```
각각의 한글 이름들은 '숫자'로 대응됩니다. (학교 이름 제외)

## Response

학교의 일부 단어만 입력하여도 됩니다.
Response 데이터는 schulList 오브젝트에 차곡차곡 관련 학교데이터가 저장됩니다   
배열 형식으로 학교들이 저장되며, 각각엔  

orgCode : 학교 코드입니다. (request시에는 한글 이름이였습니다.) 
kraOrgNm : 학교 이름입니다.  
addres : 학교 주소입니다.  
atptOfcdcConctUrl : 적합한 도 코드가 들어간 도메인입니다. (= 지역코드hcs.eduro.go.kr)

이러한 프로퍼티들이 있습니다.

그 외에도 많은 정보가 schulList 오브젝트에 담겨있습니다.  
따라서, 사용자의 선택 학교의 orgCode를 가져와 사용할 수 있습니다.

## 학교 이름
-----------
학교이름은 재학중인 학교의 이름을 쓰시면 됩니다. 

# https://지역코드hcs.eduro.go.kr/v2/findUser - 유저 찾기

## Request (Post)

```
{   
  "orgcode":"학교 코드",  
  "name":"암호화된 이름",  
  "birthday":"암호화된 생년월일(6자리 형식)",
  "loginType":"school",
  "stdntPNo":None,
}
```

## Response

```
{
  "orgName":"한글학교이름","admnYn":"N","atptOfcdcConctUrl":"도메인",     
  "mngrClassYn":"N","pInfAgrmYn":"Y","userName":"이름","stdntYn":"Y",    
  "token":"배리어 토큰","mngrDeptYn":"N"
}
```

토큰 매우 중요하니 받아올때마다 새로 저장해야합니다.

# https://지역코드hcs.eduro.go.kr/v2/hasPassword 비밀번호 설정 여부

Request 헤더의 Authorization 에 findUser가 response 한 token을 설정합니다.

## Request (Post)
```
{}
```
## Response
```
boolean
```

# https://지역코드hcs.eduro.go.kr/v2/validatePassword - 비밀번호 로그인

암호화된 비밀번호를 전송합니다.

## Request (Post)

```
{ "password":"암호화된 비밀번호","deviceUuid":""}
```

## Response 

1. 성공
  ```
  true
  ```

2. 실패
  ```
  {"isError":true,"statusCode":252,"errorCode":1001,"data":{"failCnt":실패 카운트,"canInitPassword":false}}
  ```

# https://지역코드hcs.eduro.go.kr/v2/selectUserGroup 유저 목록 얻기

Object Array 형식으로 존재하는 모든 유저가 나옵니다. 원하는 유저의 token을 저장해주세요.  

Request는 Authorization 헤더에 토큰을 넣어서, {}로 없습니다.

Response
```
{ (ex 첫째 유저0)
  많은 값들이 있으나, 
  token과 userPNo 정도만 씁니다. 
},
{ (ex 둘째 유저1)
  많은 값들이 있으나, 
  token과 userPNo 정도만 씁니다. 
}
```


# https://지역코드hcs.eduro.go.kr/v2/getUserInfo 유저 정보

selectuserGroup에서 나온 한 유저의 token과 userPNo를 사용합니다.  
Request 헤더 Authorization 업데이트합니다
## Request
```
{"orgCode":"학교코드","userPNo":userPNo}
```
## Response
```
{ token : 배리어토큰 .. 그 외 여러개 있습니다.}
```


# https://지역코드hcs.eduro.go.kr/registerServey - 설문지 제출

Request Authorization을 업데이트해주세요.

## Request
```
  {"rspns01":"1","rspns02":"1","rspns03":null,"rspns04":null,"rspns05":null,"rspns06":null,"rspns07":"0","rspns08":"0","rspns09":"0","rspns10":null,"rspns11":null,"rspns12":null,"rspns13":null,"rspns14":null,"rspns15":null,"rspns00":"Y","deviceUuid":"","upperToken":token,"upperUserNameEncpt":"이름"}
```
설문지 내용에 따라 저 rspns 값들은 달라지며, 체크박스 2개 이상인 경우, 최솟값(= 긍정)은 1이며, 2개중 한개를 택하는 설문은 긍정값(= 아니요)은 0입니다.

request 토큰은 유저정보에서 얻은 token과 같습니다.

## Response

```
{"registerDtm":"Sep 10, 2020 9:54:58 PM","inveYmd":"20200910"}
```

자가진단 날짜가 나옵니다.

## 결과
결과는 다시 유저 정보 얻기에서 isHealthy가 존재하며 값이 true 이면 확인된 것입니다.

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
