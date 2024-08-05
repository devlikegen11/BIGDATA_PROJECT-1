import numpy as np
import socket
import json
from tensorflow.keras.models import load_model

TCP_IP = '10.10.21.115'
TCP_PORT = 12344

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((TCP_IP, TCP_PORT))

# 메시지를 딕셔너리로 작성
first_msg = {"first_msg": "11"}

# 딕셔너리를 JSON 문자열로 변환
json_msg = json.dumps(first_msg)

# JSON 문자열을 인코딩하여 전송
s_msg = sock.send(json_msg.encode())

def predict_churn_batch(data_batch, model):
    pred = model.predict(np.array(data_batch))
    return pred[:, 0]

# 모델 로드
model_path = "./data/model/testai.keras"
model = load_model(model_path)

# sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
# sock.bind((TCP_IP, TCP_PORT))
# sock.listen(1)

while True:
    # conn, addr = sock.accept()
    print("검색 대기중")

    data = sock.recv(1024).decode()
    if not data:
        break
    data_json = json.loads(data)
    print(data_json)

    # 예시 데이터
    example_data = [
        data_json.get("GENDER", 0),
        data_json.get("AGE", 30),
        data_json.get("TENURE", 12),
        data_json.get("PHONE", 1),
        data_json.get("STREAMING", 0),
        data_json.get("UNLIMITED", 1),
        data_json.get("CHARGE", 70),
        data_json.get("SATISFACTION", 3)
    ]

    # 범위 지정
    age_ranges = [(19, 29), (30, 39), (40, 49), (50, 59), (60, 69), (70, 80)]
    tenure_ranges = [(1, 6), (7, 12), (13, 18), (19, 24), (25, 30), (31, 36), (37, 42), (43, 48), (49, 54), (55, 60),
                     (61, 65), (66, 72)]
    charge_ranges = [(18, 37), (38, 57), (58, 77), (78, 97), (98, 120)]

    input_age = example_data[1]
    input_tenure = example_data[2]
    input_charge = example_data[6]

    def get_range(ranges, value):
        for r in ranges:
            if r[0] <= value <= r[1]:
                return r
        return None

    age_range = get_range(age_ranges, input_age)
    tenure_range = get_range(tenure_ranges, input_tenure)
    charge_range = get_range(charge_ranges, input_charge)

    result = {}
    if age_range and tenure_range and charge_range:
        print(f"Age 범위: {age_range[0]} - {age_range[1]}")
        print(f"Tenure 범위: {tenure_range[0]} - {tenure_range[1]}")
        print(f"Charge 범위: {charge_range[0]} - {charge_range[1]}")

        no_streaming_batch = []
        with_streaming_batch = []

        for age in range(age_range[0], age_range[1] + 1):
            for tenure in range(tenure_range[0], tenure_range[1] + 1):
                for charge in range(charge_range[0], charge_range[1] + 1):
                    example_data[1] = age
                    example_data[2] = tenure
                    example_data[6] = charge

                    no_streaming_batch.append(example_data.copy())
                    example_data_with_streaming = example_data.copy()
                    example_data_with_streaming[4] = 1
                    with_streaming_batch.append(example_data_with_streaming)

        no_streaming_probs = predict_churn_batch(no_streaming_batch, model)
        with_streaming_probs = predict_churn_batch(with_streaming_batch, model)

        avg_no_streaming = float(np.mean(no_streaming_probs))
        avg_with_streaming = float(np.mean(with_streaming_probs))

        print(
            f"\n{age_range[0]} - {age_range[1]}세, Tenure {tenure_range[0]} - {tenure_range[1]}, Charge {charge_range[0]} - {charge_range[1]}에 대한 평균 값")
        print(f"스트리밍 없음 평균: {avg_no_streaming:.2f}")
        print(f"스트리밍 있음 평균: {avg_with_streaming:.2f}")

        result = {
            # "age_range": f"{age_range[0]} - {age_range[1]}",
            # "tenure_range": f"{tenure_range[0]} - {tenure_range[1]}",
            # "charge_range": f"{charge_range[0]} - {charge_range[1]}",
            "no_stream": avg_no_streaming,
            "with_stream": avg_with_streaming
        }
    else:
        result = {"error": "입력된 데이터가 범위에 맞지 않습니다."}

    result_json = json.dumps(result)
    sock.send(result_json.encode())
    print("마지막 끝")
    # sock.close()
