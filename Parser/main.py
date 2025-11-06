from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import psycopg2
import os

class DrugResponse(BaseModel):
    barcode: int
    trade_name: str
    inn: str
    package_quantity: int

app = FastAPI(title="Drug Reference API", version="1.0.0")

def get_db_connection():
    return psycopg2.connect(
        host='db',
        database='treatment',
        user='postgres',
        password='4543',
        port=5432
    )

@app.get("/api/drugs/barcode/{barcode}", response_model=DrugResponse)
def search_by_barcode(barcode: int):
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        cur.execute(
            "SELECT trade_name, inn, barcode, package_quantity FROM drugs WHERE barcode = %s", 
            (barcode,)
        )
        
        result = cur.fetchone()
        cur.close()
        conn.close()
        
        if result:
            return DrugResponse(
                trade_name=result[0],
                inn=result[1],
                barcode=result[2],
                package_quantity=result[3]
            )
        else:
            raise HTTPException(status_code=404, detail="Препарат не найден")
            
    except HTTPException:
        raise
    except Exception as e:
        print(f"Ошибка: {e}")
        raise HTTPException(status_code=500, detail="Внутренняя ошибка сервера")

@app.get("/health")
def health_check():
    return {"status": "healthy", "service": "drug-reference"}

@app.get("/api/drugs/search")
def search_by_name(name: str, limit: int = 10):
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        cur.execute(
            "SELECT trade_name, inn, barcode, package_quantity FROM drugs WHERE trade_name ILIKE %s LIMIT %s", 
            (f'%{name}%', limit)
        )
        
        results = cur.fetchall()
        cur.close()
        conn.close()
        
        drugs_list = []
        for result in results:
            drugs_list.append({
                "trade_name": result[0],
                "inn": result[1],
                "barcode": result[2],
                "package_quantity": result[3]
            })
        
        return drugs_list
            
    except Exception as e:
        print(f"Ошибка поиска: {e}")
        raise HTTPException(status_code=500, detail="Ошибка поиска")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8080)