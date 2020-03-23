import re

# noinspection PyPackageRequirements
from sqlalchemy import create_engine, and_, exc
# noinspection PyPackageRequirements
from sqlalchemy.orm import sessionmaker

import radiusd
from models.freeradius.radreply import Radreply
from models.jincreek.sim import Sim
from models.jincreek.sim import SimGroup

DB_HOST_1 = '172.16.0.122'
DB_PORT_1 = 3306
DB_USER_1 = 'jincreek-admin'
DB_PASS_1 = 'jincreek12345'
DB_NAME_1 = 'mdb'

DB_HOST_2 = '172.16.0.122'
DB_PORT_2 = 3306
DB_USER_2 = 'radius-admin'
DB_PASS_2 = 'radius12345'
DB_NAME_2 = 'radius'

ATTR_USERNAME = 'User-Name'
ATTR_CALLING_STATION_ID = 'Calling-Station-Id'
ATTR_FRAMED_IP_ADDRESS = 'Framed-IP-Address'

engine_jincreek = None
engine_radius = None

def _get_session():
#    session = sessionmaker(twophase=True)
    session = sessionmaker(autocommit=False, autoflush=True, bind=engine_radius)
    session.configure(binds={Sim: engine_jincreek, SimGroup: engine_jincreek, Radreply: engine_radius})
    return session()


def instantiate(p):
    global engine_jincreek
    global engine_radius

    print('*** instantiate ***')
    engine_jincreek = create_engine(
        'mysql+mysqlconnector://{0}:{1}@{2}:{3}/{4}'.format(DB_USER_1, DB_PASS_1, DB_HOST_1, DB_PORT_1, DB_NAME_1),
        echo=True, pool_size=20)

    engine_radius = create_engine(
        'mysql+mysqlconnector://{0}:{1}@{2}:{3}/{4}'.format(DB_USER_2, DB_PASS_2, DB_HOST_2, DB_PORT_2, DB_NAME_2),
        echo=True, pool_size=20)

    return 0


def detach(p):
    print('*** detach ***')
    print(p)

    # engine_jincreek.dispose()
    # engine_radius.dispose()

    return radiusd.RLM_MODULE_OK


def post_auth(p):
    print('*** post_auth ***')
    print(p)

    session = _get_session()

    request = dict(p)
    if ATTR_USERNAME not in request or ATTR_CALLING_STATION_ID not in request:
        return radiusd.RLM_MODULE_FAIL

    username = request[ATTR_USERNAME]
    tel_number = re.sub('^81', '0', format(request[ATTR_CALLING_STATION_ID]))

    try:
        username_arr=username.split("@")
        acct=username_arr[0]
        suff=username_arr[1]
        sim = session.query(Sim).join(SimGroup, Sim.SimGroupId == SimGroup.Id).filter(
            and_(
                Sim.UserName == acct,
                SimGroup.UserNameSuffix == suff,
                Sim.Msisdn == tel_number,
            )
        ).first()
        print(sim)

        if sim is not None:
            session.query(Radreply).filter(
                and_(
                    Radreply.username == username,
                    Radreply.attribute == ATTR_FRAMED_IP_ADDRESS,
                )
            ).delete()
            session.commit()

    except exc.SQLAlchemyError as e:
        print('SQLAlchemy ERROR: {0}'.format(str(e)))
        session.rollback()
        return radiusd.RLM_MODULE_FAIL

    finally:
        session.close()

    return radiusd.RLM_MODULE_OK
