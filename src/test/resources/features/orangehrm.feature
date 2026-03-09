Feature: Gestión de empleado en OrangeHRM

  Scenario: Crear empleado y validarlo desde Directory
    Given el usuario ingresa a OrangeHRM con credenciales de administrador
    When navega al modulo PIM
    And agrega un nuevo empleado con nombre "JuanV1" y apellido "Lopez"
    And sube una foto de perfil
    And navega al modulo Directory
    And busca el empleado creado
    Then valida que la informacion basica del empleado sea correcta