# Protocol
[Test Path](./Flutter/Test.md)

## 1. 기본구조

표의 경우 List의 하위 항목으로 작성 시 Obsidian에서 정상 출력 되지 않기 때문에 이미지로 대체

### 1.1 용어 정리

- Transport Data: Transport로 통해 전송되어오는 단위
- Protocol Frame: Protocol 송/수신 규격에 맞춰진 Data Frame
- Driver Frame: Protocol 데이터 처리 규격에 맞춰진 Data Frame, Protocol Frame 내부에 속해있는 Data에서 데이터 규격으로 이루어진 Frame을 말한다.
- Wrapper: Transport로 송/수신 하기 위해 Protocol Frame 데이터를 처리하는 Processor
- Driver: 개발자가 사용하기 편한 방식으로 변환하기 위해 Driver Frame을 분석하는 Processor
- DeviceKey: 통신에서 같은 Device Model이더라도 Device를 구분하기 위한 key
- DataKey: Protocol의 데이터의 담당을 구분하기 위한 Key
- Map: 1개의 Parameter에서 Protocol을 통해 데이터를 송/수신 할 경우 사용 DataKey, 데이터 변환 규격을 정의한 형태

### 1.2 Interface 명세

![image.png](image.png)

- Protocol에 따라 Method에 사용되는 Parameter 종류 및 개수가 다르기 때문에 공통 속성으로 정의할 수 없으나
Method구분을 위한 정의
- Wrapper
    
    
    | 명령어 | 인자(Parameter) | 반환값(return) | 설명 |
    | --- | --- | --- | --- |
    | Parse | Transport Data | Protocol Frame | 수신 된 Transport Data에서 Protocol Frame 추출 |
    | Build | Driver Frame | Protocol Frame | Driver Frame에서 Transport로 전송하기 위한 최종 Protocol Frame 처리 |
- Driver
    
    
    | 명령어 | 인자(Parameter) | 반환값(return) | 설명 |
    | --- | --- | --- | --- |
    | Extraction | Driver Frame | Map<Data Key, MetaData> | Driver Frame에서 구분자(Data Key)별 데이터 추출 |

## 2. 종류

### 2.1 Modbus

|  | Wrapper | Driver | Data Key |
| --- | --- | --- | --- |
| ModbusRTU | RTU | Standard | Standard |
| ModbusASCII | ASCII | Standard | Standard |
| ModbusTCP | TCP | Standard | Standard |
| ModbusRTU_LPCM | RTU | Standard | LPCM |
| ModbusASCII_TD300500 | ASCII | TD300, 500 | Standard |
| ModbusRTU_TDH510 | RTU | TD, TH510 | TD, TH510 |
| ModbusRTU_TDH510_EXP | RTU | TD, TH510 - EXP | TD, TH510 |
| ModbusASCII_TDH510 | ASCII | TD, TH510 | TD, TH510 |
| ModbusASCII_TDH510_EXP | ASCII | TD, TH510 - EXP | TD, TH510 |
| ModbusRTU_TS510 | RTU | TS510 | TS510 |
| ModbusRTU_TS510_EXP | RTU | TS510 - EXP | TS510 |
| ModbusASCII_TS510 | ASCII | TS510 | TS510 |
| ModbusASCII_TS510_EXP | ASCII | TS510 - EXP | TS510 |
- Class Diagram
    
    ```mermaid
    classDiagram
    ModbusWrapper <|-- RTUWrapper
    ModbusWrapper <|-- AsciiWrapper
    ModbusWrapper <|-- TCPWrapper
    
    ModbusDriver <|-- ModbusDriver_LPCM
    ModbusDriver <|-- ModbusDriver_TD300500
    ModbusDriver <|-- ModbusDriver_TDH510
    ModbusDriver_TDH510 <|-- ModbusDriver_TDH510_EXP
    ModbusDriver <|-- ModbusDriver_TS510
    ModbusDriver_TS510 <|-- ModbusDriver_TS510_EXP
    
    %% Struct
    class ModbusWrapper{
    	<<abstract>>
    	+Parse(byte[]) byte[]
    	+Build(byte, byte[]) byte[]
    	+GetDeviceAddress(byte[]) byte?
    	+ExtractPDU(byte[]) byte[]
    }
    class RTUWrapper{
    	+Verification(byte[]) bool
    	+CreateCRC(byte[]) byte[]
    }
    class AsciiWrapper{
    	+Verification(byte[]) bool
    	+CreateLRC(byte[]) byte[]
    }
    
    class ModbusDriver{
    	+ItemExtract(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#CreateKey(UInt16?, byte?, string) ModbusDataKey
    	
    	#ReadCoils(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#ReadHoldingRegister(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#WriteSingleCoils(byte[]) Map~ModbusDataKey, MetaData~
    	#WriteSingleRegister(byte[]) Map~ModbusDataKey, MetaData~
    	#WriteMultipleCoils(byte[]) Map~ModbusDataKey, MetaData~
    	#WriteMultipleRegister(byte[]) Map~ModbusDataKey, MetaData~
    	#ReadWriteMultipleRegister(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    class ModbusDriver_LPCM{
    	#CreateKey(UInt16?, byte?, string) ModbusDataKey
    }
    class ModbusDriver_TD300500{
    	#CreateKey(UInt16?, byte?, string) ModbusDataKey
    	
    	#HoldingRegisterRandomRead(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#HoldingRegisterRandomWrite(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    class ModbusDriver_TDH510{
    	#CreateKey(UInt16?, byte?, string) ModbusDataKey
    	
    	#ReadPatternSegment(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#WritePatternSegment(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    class ModbusDriver_TDH510_EXP{
    	#ReadCoils(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#ReadHoldingRegister(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    class ModbusDriver_TS510{
    	#CreateKey(UInt16?, byte?, string) ModbusDataKey
    	
    	#ReadPatternSegment(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#WritePatternSegment(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    class ModbusDriver_TS510_EXP{
    	#ReadCoils(byte[], byte[]) Map~ModbusDataKey, MetaData~
    	#ReadHoldingRegister(byte[], byte[]) Map~ModbusDataKey, MetaData~
    }
    
    %% Style
    class ModbusWrapper:::green
    
    classDef yellow fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef orange fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef blue fill:#b3e5fc,stroke:#01579b,stroke-width:2px
    classDef green fill:#f1f8e9,stroke:#558b2f,stroke-width:2px;
    ```
    
- Wrapper
    - 개요: Protocol Frame의 ADU(Address Data Unit)을 확인하거나 생성하는 컴포넌트
    - 종류
        - RTU: 3.5 CharTime, CRC
            
            ![image.png](image%201.png)
            
        - Ascii: STX, CRLF, LRC
            
            ![image.png](image%202.png)
            
        - TCP: TransactionID[2], ProtocolID[2], DataLength[2]
            
            ![image.png](image%203.png)
            
- Driver
    - 개요: Protocol Frame의 PDU(Protocol Data Unit)을 분류하거나 생성하는 컴포넌트
    - Format
        - 개요: Protocol Frame 구성
        - 종류
            1. Standard: 기본 Modbus
                - 16 Bit데이터 사용 시, Big Endian 방식으로 구성
                    - byte[0]: Hi, byte[1]: Low
                - 01(0x01), Read Coils
                02(0x02), Read Discrete Inputs
                    - 구성
                        - Request: Function[1] + Start Address H/L[2] + Read Quantity H/L[2]
                        - Response: Function[1] + Byte Count[1] + Data[Reg Quantity]
                - 03(0x03), Read Holding Registers
                04(0x04), Read Input Registers
                    - 구성
                        - Request: Function[1] + Start Address H/L[2] + Read Quantity H/L[2]
                        - Response: Function[1] + Byte Count[1] + Data H/L[Reg Quantity]
            2. LPCM: Standard
                - Key에 FunctionCode 사용
            3. TD300, 500: Standard + 전용 FunctionCode
                - 103: Holding Register Random Read
                - 106: Holding Register Random Write
            4. TD, TH510: Standard + Hook(23, 26)
                - 23: 기존 Modbus FunctionCode 덮어쓰기, Pattern 편집 Command
                - 26: Pattern 편집 Command
            5. TD, TH510_EXP: Standard + Hook(23, 26) + EXP Hook(1~4)
                - 23: 기존 Modbus FunctionCode 덮어쓰기, Pattern 편집 Command
                - 26: Pattern 편집 Command
                - EXP: 기존 Modbus FunctionCode 덮어쓰기, Byte Count 1 → 2
            6. TS510: Standard + Hook(23, 26)
                - 23: 기존 Modbus FunctionCode 덮어쓰기, Pattern 편집 Command
                - 26: Pattern 편집 Command
            7. TS510_EXP: Standard + Hook(23, 26) + EXP Hook(1~4)
                - 23: 기존 Modbus FunctionCode 덮어쓰기, Pattern 편집 Command
                - 26: Pattern 편집 Command
                - EXP: 기존 Modbus FunctionCode 덮어쓰기, Byte Count 1 → 2
- Key
    - 개요: PDU(Protocol Data Unit)에서 각 Data를 담은 Register를 구분하는 구분자
    - 종류
        1. Standard: Reference Number(기본 Modbus Registry Address) 구조만 사용
        2. LPCM: FunctionCode + Registry Address
        3. TD, TH510: Stadnard + 전용 Custom FunctionCode Key
            - 23, 26: 패턴, 세그먼트 값 읽기
                - Pattern 번호[Word]
                - Segment 번호[Word]
                - Target 목록
                    - [DWord] - Ch01 SV
                    - [DWord] - Ch02 SV
                    - [Word] - Segemnt 시간 - Hour
                    - [Word] - Segemnt 시간 - Minute
                    - [Word] - 대기설정 여부
                    - [Word] - Time Signal 1 사용 여부
                    - [Word] - Time Signal 2 사용 여부
                    - [Word] - Time Signal 3 사용 여부
                    - [Word] - Time Signal 4 사용 여부
                    - [Word] - Alarm 1 사용 여부
                    - [Word] - Alarm 2 사용 여부
                    - [Word] - Alarm 3 사용 여부
                    - [Word] - Alarm 4 사용 여부
        4. TS510: Stadnard + 전용 Custom FunctionCode Key
            - 23, 26: 패턴, 세그먼트 값 읽기
                - Pattern 번호[Word]
                - Test Room 번호[Word]
                    - 고온실: 1 / 저온실: 2 / 시험실: 3
                - Target 목록
                    - [DWord] - Pre SV
                    - [DWord] - TSV SV
                    - [DWord] - Wait Temp
                    - [Word] - 운전 시간 - Hour
                    - [Word] - 운전 시간 - Minute
                    - [Word] - 운전 시간 - Seconds
                    - [Word] - Wait 시간 -  Minute
                    - [Word] - 대기설정 여부
                    - [Word] - Time Signal 1 사용 여부
                    - [Word] - Time Signal 2 사용 여부
                    - [Word] - Time Signal 3 사용 여부
                    - [Word] - Time Signal 4 사용 여부
- Exception
    
    
    | 에러코드 | 내용 | 비고 |
    | --- | --- | --- |
    | 01 | 잘못된 FunctionCode |  |
    | 02 | 잘못된 Register |  |
    | 03 | 데이터 개수 오류 |  |
    | 04 | Data 오류 |  |
    | 21 | Full Buffer |  |

### 2.2 PCLink

|  | Wrapper | Driver | Data Key |
| --- | --- | --- | --- |
| PCLinkSTD | Standard | Standard | Standard |
| PCLinkSUM | SUM | Standard | Standard |
| PCLinkSUM_TD300500 | SUM | TD300, 500 | TD300, 500 |
| PCLinkSTD_TH300500 | TH300, 500 | TH300, 500 | TH300, 500 |
| PCLinkSUM_TH300500 | TH300, 500 SUM | TH300, 500 | TH300, 500 |
| PCLinkSTD_TDH510 | Standard | TD, TH510 | TD, TH510 |
| PCLinkSUM_TDH510 | SUM | TD, TH510 | TD, TH510 |
| PCLinkSTD_TS510 | Standard | TS510 | TS510 |
| PCLinkSUM_TS510 | SUM | TS510 | TS510 |
- Class Diagram
    
    ```mermaid
    classDiagram
    PCLinkWrapper <|-- STDWrapper
    PCLinkWrapper <|-- STDWrapper_TH300500
    PCLinkWrapper <|-- SUMWrapper
    PCLinkWrapper <|-- SUMWrapper_TH300500
    
    IPCLinkDriver <|-- PCLinkDriver
    IPCLinkDriver <|-- PCLinkDriver_TD300500
    IPCLinkDriver <|-- PCLinkDriver_TH300500
    PCLinkDriver <|-- PCLinkDriver_TDH510
    PCLinkDriver <|-- PCLinkDriver_TS510
    
    %% Struct
    class PCLinkWrapper {
    	<<abstract>>
    	+Parse(byte[]) byte[]
    	+Build(byte, byte[]) byte[]
    	+ExtractPDU(byte[]) byte[]
    }
    class SUMWrapper{
    	+Verification(byte[]) bool
    	+CreateCheckSum(byte[]) byte[]
    }
    class SUMWrapper_TH300500{
    	+Verification(byte[]) bool
    	+CreateCheckSum(byte[]) byte[]
    }
    
    class IPCLinkDriver{
    	<<interface>>
    	ItemExtract(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	GetAddress(string) int?
    	GetCommand(string) string
    }
    
    class PCLinkDriver{
    	#DRS(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#DRR(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#IRS(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#IRR(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#DWS(byte[]) Map~PCLinkDataKey, MetaData~
    	#DWR(byte[]) Map~PCLinkDataKey, MetaData~
    	#IWS(byte[]) Map~PCLinkDataKey, MetaData~
    	#IWR(byte[]) Map~PCLinkDataKey, MetaData~
    }
    class PCLinkDriver_TS510{
    	#RSD(byte[]) Map~PCLinkDataKey, MetaData~
    	#WSD(byte[]) Map~PCLinkDataKey, MetaData~
    }
    class PCLinkDriver_TDH510{
    	#RSD(byte[]) Map~PCLinkDataKey, MetaData~
    	#WSD(byte[]) Map~PCLinkDataKey, MetaData~
    }
    class PCLinkDriver_TD300500{
    	#RDR(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#RRD(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#WDR(byte[]) Map~PCLinkDataKey, MetaData~
    	#WRD(byte[]) Map~PCLinkDataKey, MetaData~
    }
    class PCLinkDriver_TH300500{
    	#RRP(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#RUP(byte[], byte[]) Map~PCLinkDataKey, MetaData~
    	#RTD(byte[]) Map~PCLinkDataKey, MetaData~
    	#RCV(byte[]) Map~PCLinkDataKey, MetaData~
    }
    
    %% Style
    class PCLinkWrapper:::green
    class IPCLinkDriver:::yellow
    
    classDef yellow fill:#fffde7,stroke:#fbc02d,stroke-width:2px;
    classDef orange fill:#fff3e0,stroke:#ef6c00,stroke-width:2px;
    classDef blue fill:#b3e5fc,stroke:#01579b,stroke-width:2px
    classDef green fill:#f1f8e9,stroke:#558b2f,stroke-width:2px;
    ```
    
- Wrapper
    - 개요: Protocol Frame의 Header, Tail을 확인하거나 생성하는 컴포넌트
    - 종류
        1. Standard
            
            ![image.png](image%204.png)
            
        2. SUM
            
            ![image.png](image%205.png)
            
        3. TH300500
            
            ![image.png](image%206.png)
            
        4. TH300500_SUM
            
            ![image.png](image%207.png)
            
- Driver
    - 개요: Protocol Frame의 Body에있는 데이터를 분류하거나 생성하는 컴포넌트
    - Format
        - 종류
            1. Standard: 기본 PCLink
                
                ![image.png](image%208.png)
                
            2. TD300500: TD300, 500 전용 명령어, Standard를 사용하지 않음
                
                ![image.png](image%209.png)
                
            3. TH300500: TH300, 500 전용 명령어, Standard를 사용하지 않음
                
                ![image.png](image%2010.png)
                
            4. TD, TH510: Stadard + TD, TH510 전용 명령어
                - RSD, WSD: 패턴편집 Command
            5. TS510: Stadard + TS510 전용 명령어
                - RSD, WSD: 패턴편집 Command
- Key
    - 개요: Driver Frame의 Body에서 각 Data Block을 구분하는 구분자
    - 종류
        1. Standard: Registry Address 만을 사용
        2. TD300500: Stadnard + 전용 Custom Command Key
            - RPD: Read Pattern Data
                - Pattern 번호
                - Segment 번호
                - Target 목록
                    - [Word] - Pattern Size
                    - [Word] - Link Pattern
                    - [Word] - Wait - Hour
                    - [Word] - Wait - Minute
                    - [DWord] - Repeat Count
                    - [DWord] - Wait Temp
                    - [Word] - StartEndMode
                        - Start Mode: Bit 0 ~ 3
                        - End Mode: Bit 4 ~ 6
                        - Time Mode: Bit 7
                    - [Word] - Pattern End Segment
                    - [DWord] - StartSV1
                    - [DWord] - StartSV2
                    - [String] - Pattern Name
                    - [Word * 3 * 20] - Loop 정보 * 20
                        - [Word] - Start Segment
                        - [Word] - End Segment
                        - [Word] - Loop Count
            - RSD: Read Segment Data
                - Pattern 번호
                - Segment 번호
                - Target 목록
                    - [DWord] - SV1
                    - [DWord] - SV2
                    - [Word] - 대기 여부
                    - [Word] - Segment Time - Hour
                    - [Word] - Segment Time - Minute
                    - [Word] - PID NO
                    - [Word * 5 * 8] - Time Signal 정보 * 8
                        - [Word] - Time Signal Mode
                        - [Word] - Delay Time - Hour
                        - [Word] - Delay Time - Minute
                        - [Word] - On Time - Hour
                        - [Word] - On Time - Minute
        3. TH300500: 전용 Custom Key
            - Command + Registry Address구조
            - Command에 따른 고정 대상 정보
                - RPD: Read Pattern Data
                    - Pattern 번호
                    - Registry 번호
                - RSD: Read Segment Data
                    - Pattern 번호
                    - Segment 번호
                    - Registry 번호
                - RCV: Read Current Value
                    - Target 목록
                        - [2 Byte] - T.SV
                        - [2 Byte] - T.PV
                        - [2 Byte] - T.MV
                        - [2 Byte] - H.SV
                        - [2 Byte] - H.PV
                        - [2 Byte] - H.V
                        - [2 Byte] - T_I/S
                        - [2 Byte] - D_T/S
                        - [2 Byte] - A/S
                        - [2 Byte] - RY
                        - [2 Byte] - O/C
                        - [2 Byte] - D/I
                        - [1 Byte] - RM
                        - [2 Byte] - RTH
                        - [1 Byte] - RTM
                        - [1 Byte] - RTS
                        - [2 Byte] - SRTH
                        - [1 Byte] - SRTM
                        - [2 Byte] - SFTH
                        - [1 Byte] - SFTM
                        - [2 Byte] - RPTN
                        - [1 Byte] - RSEG
                        - [2 Byte] - RPRC
                        - [2 Byte] - RPRN
                        - [1 Byte] - RLC
                        - [1 Byte] - RLN
                        - [2 Byte] - UDSW
        4. TDH510: Standard + 전용 Custom Command Key
            - RSD
                - Pattern 번호[Word]
                - Segment 번호[Word]
                - Target 목록
                    - [DWord] - Ch01 SV
                    - [DWord] - Ch02 SV
                    - [Word] - Segemnt 시간 - Hour
                    - [Word] - Segemnt 시간 - Minute
                    - [Word] - 대기설정 여부
                    - [Word] - Time Signal 1 사용 여부
                    - [Word] - Time Signal 2 사용 여부
                    - [Word] - Time Signal 3 사용 여부
                    - [Word] - Time Signal 4 사용 여부
                    - [Word] - Alarm 1 사용 여부
                    - [Word] - Alarm 2 사용 여부
                    - [Word] - Alarm 3 사용 여부
                    - [Word] - Alarm 4 사용 여부
        5. TS510: Standard + 전용 Custom Command Key
            - RSD
                - Pattern 번호[Word]
                - Test Room 번호[Word]
                    - 고온실: 1 / 저온실: 2 / 시험실: 3
                - Target 목록
                    - [DWord] - Pre SV
                    - [DWord] - TSV SV
                    - [DWord] - Wait Temp
                    - [Word] - 운전 시간 - Hour
                    - [Word] - 운전 시간 - Minute
                    - [Word] - 운전 시간 - Seconds
                    - [Word] - Wait 시간 -  Minute
                    - [Word] - 대기설정 여부
                    - [Word] - Time Signal 1 사용 여부
                    - [Word] - Time Signal 2 사용 여부
                    - [Word] - Time Signal 3 사용 여부
                    - [Word] - Time Signal 4 사용 여부
- Exception
    
    
    | 에러코드 | 내용 | 비고 |
    | --- | --- | --- |
    | 01 | 잘못된 Command |  |
    | 02 | 잘못된 Register |  |
    | 03 | Register 지정범위 초과 |  |
    | 04 | Data 검정 오류 | 16진수 문자가 아닌 문자가 사용됨 |
    | 08 | Format 오류 | Command에 해당되는 Format이 아님 |
    | 16 | CheckSUM 오류 |  |
    | 00 | 기타 |  |
