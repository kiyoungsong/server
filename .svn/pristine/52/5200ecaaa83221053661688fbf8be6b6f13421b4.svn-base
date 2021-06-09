from System.Collections.Generic import *
#from System.Collections import BitArray
import binascii

isSwap = False
transactionIdentifier = 0 

##initSize
def InitSize():
    result = {  
                "readMaxSize" : 1000,          ##Data Read Max Size
                "writeMaxSize" : 1000,           ##Data Write Max Size
                "headerLength" : 20,          ##Command Header Length (i think driver doesn't need)
                "bodyLength" : 0,             ##Command body Length     
                "readCommandLength" : 9,     ##Command Read Total Length
                "writeCommandLength" : 22,    ##Comannd Write Total Length
                "resHeader" : 0,               ##Reponse Read/Write Command Length 
                "isBit" : 1 }                 ##Bit Address not Byte Address : 1 / Bit Address equal byte Address : 0
    return Dictionary[str, int](result)


def InitValue():
    ##Device Values coil, input, ....
    devices = { 'coil' : "\x01", 'input' : "\x02", 'holdingregister': "\x03", 'inputregister' : "\x04" }
    
    return Dictionary[str, str](devices)


##Packet
def MakePacket(byCMD, device, dwRegNum, dwStartAddr, offset, writeBuff):
    packet = ""

    global transactionIdentifier
    #transactionIdentifier
    if transactionIdentifier == 65535:
        transactionIdentifier = 1
    else:
        transactionIdentifier += 1
    
    hexNum = hex(transactionIdentifier)
    temp = hexNum.replace("0x","")
    if (len(temp)-1)/2 == 0:
        temp += "00"
    
    if len(temp)%2 == 1:
        trans = binascii.a2b_hex('0'+temp)
    else:
        trans = binascii.a2b_hex(temp)

    packet += trans[1:2]
    packet += trans[:1]
    #protocolIdentifier
    packet += "\x00"
    packet += "\x00"
    #length
    packet += "\x00"
    packet += "\x06"
    #unitIdentifier
    packet += "\x01"
    #functionCode (Device Type)
    hexNum = hex(device)
    deviceType = hexNum.replace("0x","")
    if len(hexNum)%2 == 1:
        packet += binascii.a2b_hex('0'+deviceType)
    else:
        packet += binascii.a2b_hex(deviceType)
    #startingAddress (Device Address)
    hexNum = hex(dwStartAddr)
    temp = hexNum.replace("0x","")
    if (len(hexNum)-3)/2 == 0:
        temp += "00"
        
    if len(temp)%2 == 1:
        temp = "0" + temp
        hexAddr = binascii.a2b_hex(temp)
    else:
        hexAddr = binascii.a2b_hex(temp)

    packet += hexAddr[1:2]
    packet += hexAddr[:1]

    #quantity (Device Length)
    hexNum = hex(dwRegNum)
    temp = hexNum.replace("0x","")
    if (len(hexNum)-3)/2 == 0:
        temp += "00"
        
    if len(temp)%2 == 1:
        temp = "0" + temp
        hexAddr = binascii.a2b_hex(temp)
    else:
        hexAddr = binascii.a2b_hex(temp)

    packet += hexAddr[1:2]
    packet += hexAddr[:1]
    #CRC
    packet += "\x84"
    packet += "\x0a"

    return packet

def ExportData(packet, dwRegNum):
    tempByte = bytearray(packet)
    byteArray = tempByte[9:]
    if len(byteArray)%2 == 1:
        byteArray.extend('\x00')
    
    global isSwap
    if isSwap:
        for i in range(0, len(byteArray), 2):    #Swap
            byteArray[i], byteArray[i + 1] = byteArray[i + 1], byteArray[i]
    
    decodeArray = byteArray.decode('utf-16')
    tempPacket = decodeArray[:dwRegNum]

    return tempPacket