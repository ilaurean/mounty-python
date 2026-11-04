FROM quay.io/keycloak/keycloak:26.8
COPY import.json /opt/keycloak/data/import/mountypython-realm.json
#COPY themes/mountypython/ /opt/keycloak/themes/mountypython/

CMD ["start", "--import-realm"]
