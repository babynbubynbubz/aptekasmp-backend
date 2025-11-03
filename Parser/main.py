from sqlalchemy import create_engine
import pandas as pd
import fastapi
from pydantic import BaseModel
import uvicorn

class Response(BaseModel):
    barcode: int
    trade_name: str
    inn: str

app = fastapi.FastAPI()

@app.get("/api/drugs/barcode/{barcode}", response_model=Response)
def search_by_barcode(barcode):
    db_config = {
        'host': 'localhost',
        'port': 5432,
        'database': 'treatment',
        'user': 'postgres',
        'password': '4543'
    }

    engine = create_engine(
        f"postgresql://{db_config['user']}:{db_config['password']}@"
        f"{db_config['host']}:{db_config['port']}/{db_config['database']}"
    )

    query = f"""
    SELECT trade_name, inn, barcode
    FROM drugs 
    WHERE barcode = {barcode}
    """

    try:
        result = pd.read_sql(query, engine)

        if not result.empty:
            product = result.iloc[0]
            return Response(inn=product['inn'], barcode=product['barcode'], trade_name=product['trade_name'])
        else:
            return None

    except Exception as e:
        print(e)
        return None
