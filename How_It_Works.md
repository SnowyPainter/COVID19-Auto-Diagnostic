# Health Machine, How does it work?

# Feature
1. 교육청 도메인만 다르고, 작동 파일부터 Request까지 다 같습니다
2. 모든 쿠키 유지합니다
3. 모든 요청은 POST형식입니다.
4. 모든 결과는 resultSVO에 담겨져있습니다.
# Res&Req sequence
1. stv_cvd_co00_004.do 에서 학교의 번호를 가져옵니다.
2. (ajax)stv_cvd_co00_012.do 으로 정보를 보내서 필요값들을 수신합니다.
3. (form)stv_cvd_co00_000.do 에 필수정보 전송후 설문지 전달받습니다.
4. (ajax)stv_cvd_co01_000.do 으로 다시 정보 보낸후 인증값 수신합니다.
5. (form)stv_cvd_co02_000.do 같은 전송값으로 보낸후 결과값 수신합니다.
# Works more details

## 1. stv_cvd_co00_004.do 학교검사

| Reqeust Header
--------
```
POST /stv_cvd_co00_004.do HTTP/1.1  
Host: eduro.gbe.kr
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/| 20100101 Firefox/79.0
Accept: application/json, text/javascript, */*; q=0.01
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
X-Requested-With: XMLHttpRequest
Content-Length: 53
Origin: https://eduro.gbe.kr
Connection: keep-alive
Referer: https://eduro.gbe.kr/stv_cvd_co00_002.do
Cookie: WMONID=VPLR2pWm7-P;  JSESSIONID=1CH5w3J7ZaaQ4oE92RztfnXuNnddKafZ91vejEMJ1S2O7tJOVlYWskbIjnyBrlqn.gbe-pacwas2_servlet_pacwas
```
| Request Body
-----------
```
{"schulNm":"학교이름"}
```

resultSVO.rtnRsltCode
"" 라면 실패입니다.
SUCCESS면 성공이고, schulCode에 학교 번호가 적혀져있습니다.

* resultSVO.schulCode 저장

------------------------------------------
## 2. stv_cvd_co00_012.do 필수 정보 받기

| Request Header
-----------------
```
POST /stv_cvd_co00_012.do HTTP/1.1
Host: eduro.gbe.kr
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/| 20100101 Firefox/79.0
Accept: application/json, text/javascript, */*; q=0.01
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
X-Requested-With: XMLHttpRequest
Content-Length: 169
Origin: https://eduro.gbe.kr
Connection: keep-alive
Referer: https://eduro.gbe.kr/stv_cvd_co00_002.do
Cookie: WMONID=VPLR2pWm7-P;  JSESSIONID=9dM1D8JJVTPl1k6s3w2MiEtNeuCnKHd19tVjLDDXK4FeaRPqVXJzltdi3kHnkWza.gbe-pacwas1_servlet_pacwas
```
| Request Body
---------------
```
{"qstnCrtfcNoEncpt":"","rtnRsltCode":"","schulCode":schulCode,"schulNm":"학교이름","pName":"이름","frnoRidno":"생년월일","aditCrtfcNo":""}
```
resultSVO.rtnRsltCode 
"" 면 실패이고, SUCCESS면 성공입니다.

* resultSVO.qstnCrtfcNoEncpt 값 저장합니다

--------------------------------------------
## 3. stv_cvd_co00_000.do 이동 후 설문지

| Request Header
--------------------
```
POST /stv_cvd_co00_000.do HTTP/1.1
Host: eduro.gbe.kr
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded
Content-Length: 208
Origin: https://eduro.gbe.kr
Connection: keep-alive
Referer: https://eduro.gbe.kr/stv_cvd_co00_002.do
Cookie: WMONID=VPLR2pWm7-P; JSESSIONID=9dM1D8JJVTPl1k6s3w2MiEtNeuCnKHd19tVjLDDXK4FeaRPqVXJzltdi3kHnkWza.gbe-pacwas1_servlet_pacwas
Upgrade-Insecure-Requests: 1
```
| Request Body
----------------
```
{"qstnCrtfcNoEncpt":resultSVO.qstnCrtfcNoEncpt,"rtnRsltCode":"SUCCESS","schulCode":schulCode,"schulNm":"학교","pName":"이름","frnoRidno":"생년월일","aditCrtfcNo":""}
```
Response 는 json이 아니라 html입니다. 설문지 내용은 학교마다 다를 수 있습니다.

------------------------------------------------
## stv_cvd_co01_000.do 에 ajax 통신후 새로이 저장

* resultSVO.schulNm
* resultSVO.stdntName
* resultSVO.qstnCrtfcNoEncpt
* resultSVO.rtnRsltCode

| Request Header
------------------
```
POST /stv_cvd_co01_000.do HTTP/1.1
Host: eduro.gbe.kr
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0
Accept: application/json, text/javascript, */*; q=0.01
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
X-Requested-With: XMLHttpRequest
Content-Length: 139
Origin: https://eduro.gbe.kr
Connection: keep-alive
Referer: https://eduro.gbe.kr/stv_cvd_co00_000.do
Cookie: WMONID=VPLR2pWm7-P; JSESSIONID=9dM1D8JJVTPl1k6s3w2MiEtNeuCnKHd19tVjLDDXK4FeaRPqVXJzltdi3kHnkWza.gbe-pacwas1_servlet_pacwas
```
| Request Body
----------------
```
{
  "rtnRsltCode":resultSVO.rtnRsltCode,
  "qstnCrtfcNoEncpt":resultSVO.qstnCrtfcNoEncpt,
  "schulNm":resultSVO.schulNm,
  "stdntName":resultSVO.stdntName ,
  "rspns01":"1","rspns02":"1","rspns07":"0","rspns08":"0","rspns09":"0"
}
```
rspnsN은 설문지 값입니다.

설문지 내용이 Yes or No 라면 0, 1로 구분됩니다.
또한 여러개의 값일 경우 1부터 시작합니다 즉, 3번째에서 받은 설문지를 
파싱해서 설문지 단위별로 갯수를 확인하면서 0이 긍정일 경우
아닌 것을 체크 할 수 있습니다.

---------------------------------------------
## stv_cvd_co02_000.do 같은 값 전송후 결과 받기

헤더만 다르고 전송하는 값은 같습니다. Html으로 응답합니다.
| Request Header
----------------
```
POST /stv_cvd_co02_000.do HTTP/1.1
Host: eduro.gbe.kr
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate, br
Content-Type: application/x-www-form-urlencoded
Content-Length: 211
Origin: https://eduro.gbe.kr
Connection: keep-alive
Referer: https://eduro.gbe.kr/stv_cvd_co00_000.do
Cookie: WMONID=VPLR2pWm7-P; JSESSIONID=9dM1D8JJVTPl1k6s3w2MiEtNeuCnKHd19tVjLDDXK4FeaRPqVXJzltdi3kHnkWza.gbe-pacwas1_servlet_pacwas
Upgrade-Insecure-Requests: 1
```
마지막으로, 결괏말 html은 #content_detail1의 첫번째 p 태그의 innerText입니다.
